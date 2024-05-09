using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MaterialSkin.Controls;
using MaterialSkin.Properties;

namespace MaterialSkin
{
    public enum Glyphs
    {
        GlobalNavigationButton = 0xE700,
        Pin = 0xE718,
        Pinned = 0xE840
    }

    public class MaterialSkinManager
    {

        private static MaterialSkinManager _instance;

        private readonly List<MaterialForm> _formsToManage = new();

        public delegate void SkinManagerEventHandler(object sender);

        public event SkinManagerEventHandler ColorSchemeChanged;

        public event SkinManagerEventHandler ThemeChanged;

        /// <summary>
        /// Set this property to false to stop enforcing the backcolor on non-materialSkin components
        /// </summary>
        public readonly bool EnforceBackcolorOnAllComponents = true;

        public static MaterialSkinManager Instance => _instance ??= new MaterialSkinManager();

        public static MaterialSkinManager ControlInstance => new();

        public const int FORM_PADDING = 14;

        /// <summary>
        /// Possible options
        /// Roboto, Roboto Light, Roboto Medium
        /// </summary>
        public string CurrentFontFamily { get; set; }
        public Font DefaultFont { get; set; }

        #region Button options
        private int _cornerRadius = 4;
        public int CornerRadius
        {
            get => _cornerRadius;
            set => _cornerRadius = value;
        }

        private Font _buttonFont;
        public Font ButtonFont { 
            get => _buttonFont;
            set
            {
                var font = new Font(GetFontFamily(CurrentFontFamily), value.SizeInPoints, value.Style, GraphicsUnit.Point);
                _buttonFont = font;
            }
        }

        private int _height = 36;
        /// <summary>
        /// Height for dialog buttons
        /// </summary>
        public int Height { 
            get => _height; 
            set => _height = value;
        }
        #endregion

        // Constructor
        private MaterialSkinManager()
        {
            Theme = Themes.Light;
            ColorScheme = new ColorScheme(Primary.Indigo500, Primary.Indigo700, Primary.Indigo100, Accent.Pink200, TextShade.White, Primary.Green700, Primary.Cyan700, Primary.Yellow700, Primary.Red700);
            CurrentFontFamily = "Roboto";

            // Create and cache Roboto fonts
            // Thanks https://www.codeproject.com/Articles/42041/How-to-Use-a-Font-Without-Installing-it
            // And https://www.codeproject.com/Articles/107376/Embedding-Font-To-Resources

            // Add font to system table in memory and save the font family
            AddFont(Resources.Roboto_Thin);
            AddFont(Resources.Roboto_Light);
            AddFont(Resources.Roboto_Regular);
            AddFont(Resources.Roboto_Medium);
            //addFont(Resources.Roboto_Bold);
            AddFont(Resources.Roboto_Black);

            AddFont(Resources.MaterialIcons_Regular);
            AddFont(Resources.MaterialIconsOutlined_Regular);
            AddFont(Resources.MaterialIconsRound_Regular);
            AddFont(Resources.MaterialIconsSharp_Regular);

            _robotoFontFamilies = new Dictionary<string, FontFamily>();
            foreach (var ff in _privateFontCollection.Families.ToArray())
            {
                _robotoFontFamilies.Add(ff.Name.Replace(' ', '_'), ff);
            }

            // create and save font handles for GDI
            _logicalFonts = new Dictionary<string, IntPtr>(18)
            {
                { "H1", CreateLogicalFont("Roboto Light", 96, NativeTextRenderer.logFontWeight.FW_LIGHT) },
                { "H2", CreateLogicalFont("Roboto Light", 60, NativeTextRenderer.logFontWeight.FW_LIGHT) },
                { "H3", CreateLogicalFont("Roboto", 48, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                { "H4", CreateLogicalFont("Roboto", 34, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                { "H5", CreateLogicalFont("Roboto", 24, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                { "H6", CreateLogicalFont("Roboto Medium", 20, NativeTextRenderer.logFontWeight.FW_MEDIUM) },
                { "Subtitle1", CreateLogicalFont("Roboto", 16, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                { "Subtitle2", CreateLogicalFont("Roboto Medium", 14, NativeTextRenderer.logFontWeight.FW_MEDIUM) },
                { "SubtleEmphasis", CreateLogicalFont("Roboto", 12, NativeTextRenderer.logFontWeight.FW_NORMAL, 1) },
                { "Body1", CreateLogicalFont("Roboto", 16, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                { "Body2", CreateLogicalFont("Roboto", 14, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                { "Button", CreateLogicalFont("Roboto Medium", 14, NativeTextRenderer.logFontWeight.FW_MEDIUM) },
                { "ButtonIcon", CreateLogicalFont("Material Icons", 14, NativeTextRenderer.logFontWeight.FW_MEDIUM) },
                { "Caption", CreateLogicalFont("Roboto", 12, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                { "Overline", CreateLogicalFont("Roboto", 10, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                { "MenuStrip", CreateLogicalFont("Roboto", 10, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                // Logical fonts for textbox animation
                { "textBox16", CreateLogicalFont("Roboto", 16, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                { "textBox15", CreateLogicalFont("Roboto", 15, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                { "textBox14", CreateLogicalFont("Roboto", 14, NativeTextRenderer.logFontWeight.FW_REGULAR) },
                { "textBox13", CreateLogicalFont("Roboto Medium", 13, NativeTextRenderer.logFontWeight.FW_MEDIUM) },
                { "textBox12", CreateLogicalFont("Roboto Medium", 12, NativeTextRenderer.logFontWeight.FW_MEDIUM) }
            };

            DefaultFont = GetFontByType(FontType.Body1);
        }

        // Destructor
        ~MaterialSkinManager()
        {
            // RemoveFontMemResourceEx
            foreach (var handle in _logicalFonts.Values)
            {
                NativeTextRenderer.DeleteObject(handle);
            }
        }

        // Themes
        private Themes _theme;

        public Themes Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                UpdateBackgrounds();
                ThemeChanged?.Invoke(this);
            }
        }

        private ColorScheme _colorScheme;

        public ColorScheme ColorScheme
        {
            get => _colorScheme;
            set
            {
                _colorScheme = value;
                UpdateBackgrounds();
                ColorSchemeChanged?.Invoke(this);
            }
        }

        public enum Themes : byte
        {
            Light,
            Dark
        }

        #region Variables static readonly
        // Text
        private static readonly Color TEXT_HIGH_EMPHASIS_LIGHT = Color.FromArgb(222, 255, 255, 255); // Alpha 87%
        private static readonly Brush TEXT_HIGH_EMPHASIS_LIGHT_BRUSH = new SolidBrush(TEXT_HIGH_EMPHASIS_LIGHT);
        private static readonly Color TEXT_HIGH_EMPHASIS_DARK = Color.FromArgb(222, 0, 0, 0); // Alpha 87%
        private static readonly Brush TEXT_HIGH_EMPHASIS_DARK_BRUSH = new SolidBrush(TEXT_HIGH_EMPHASIS_DARK);

        private static readonly Color TEXT_HIGH_EMPHASIS_LIGHT_NOALPHA = Color.FromArgb(255, 255, 255, 255); // Alpha 100%
        private static readonly Brush TEXT_HIGH_EMPHASIS_LIGHT_NOALPHA_BRUSH = new SolidBrush(TEXT_HIGH_EMPHASIS_LIGHT_NOALPHA);
        private static readonly Color TEXT_HIGH_EMPHASIS_DARK_NOALPHA = Color.FromArgb(255, 0, 0, 0); // Alpha 100%
        private static readonly Brush TEXT_HIGH_EMPHASIS_DARK_NOALPHA_BRUSH = new SolidBrush(TEXT_HIGH_EMPHASIS_DARK_NOALPHA);

        private static readonly Color TEXT_MEDIUM_EMPHASIS_LIGHT = Color.FromArgb(153, 255, 255, 255); // Alpha 60%
        private static readonly Brush TEXT_MEDIUM_EMPHASIS_LIGHT_BRUSH = new SolidBrush(TEXT_MEDIUM_EMPHASIS_LIGHT);
        private static readonly Color TEXT_MEDIUM_EMPHASIS_DARK = Color.FromArgb(153, 0, 0, 0); // Alpha 60%
        private static readonly Brush TEXT_MEDIUM_EMPHASIS_DARK_BRUSH = new SolidBrush(TEXT_MEDIUM_EMPHASIS_DARK);

        private static readonly Color TEXT_DISABLED_OR_HINT_LIGHT = Color.FromArgb(97, 255, 255, 255); // Alpha 38%
        private static readonly Brush TEXT_DISABLED_OR_HINT_LIGHT_BRUSH = new SolidBrush(TEXT_DISABLED_OR_HINT_LIGHT);
        private static readonly Color TEXT_DISABLED_OR_HINT_DARK = Color.FromArgb(97, 0, 0, 0); // Alpha 38%
        private static readonly Brush TEXT_DISABLED_OR_HINT_DARK_BRUSH = new SolidBrush(TEXT_DISABLED_OR_HINT_DARK);

        // Dividers and thin lines
        private static readonly Color DIVIDERS_LIGHT = Color.FromArgb(30, 255, 255, 255); // Alpha 30%
        private static readonly Brush DIVIDERS_LIGHT_BRUSH = new SolidBrush(DIVIDERS_LIGHT);
        private static readonly Color DIVIDERS_DARK = Color.FromArgb(30, 0, 0, 0); // Alpha 30%
        private static readonly Brush DIVIDERS_DARK_BRUSH = new SolidBrush(DIVIDERS_DARK);
        private static readonly Color DIVIDERS_ALTERNATIVE_LIGHT = Color.FromArgb(153, 255, 255, 255); // Alpha 60%
        private static readonly Brush DIVIDERS_ALTERNATIVE_LIGHT_BRUSH = new SolidBrush(DIVIDERS_ALTERNATIVE_LIGHT);
        private static readonly Color DIVIDERS_ALTERNATIVE_DARK = Color.FromArgb(153, 0, 0, 0); // Alpha 60%
        private static readonly Brush DIVIDERS_ALTERNATIVE_DARK_BRUSH = new SolidBrush(DIVIDERS_ALTERNATIVE_DARK);

        // Checkbox / Radio / Switches
        private static readonly Color CHECKBOX_OFF_LIGHT = Color.FromArgb(138, 0, 0, 0);
        private static readonly Brush CHECKBOX_OFF_LIGHT_BRUSH = new SolidBrush(CHECKBOX_OFF_LIGHT);
        private static readonly Color CHECKBOX_OFF_DARK = Color.FromArgb(179, 255, 255, 255);
        private static readonly Brush CHECKBOX_OFF_DARK_BRUSH = new SolidBrush(CHECKBOX_OFF_DARK);
        private static readonly Color CHECKBOX_OFF_DISABLED_LIGHT = Color.FromArgb(66, 0, 0, 0);
        private static readonly Brush CHECKBOX_OFF_DISABLED_LIGHT_BRUSH = new SolidBrush(CHECKBOX_OFF_DISABLED_LIGHT);
        private static readonly Color CHECKBOX_OFF_DISABLED_DARK = Color.FromArgb(77, 255, 255, 255);
        private static readonly Brush CHECKBOX_OFF_DISABLED_DARK_BRUSH = new SolidBrush(CHECKBOX_OFF_DISABLED_DARK);

        // Switch specific
        private static readonly Color SWITCH_OFF_THUMB_LIGHT = Color.FromArgb(255, 255, 255, 255);
        private static readonly Color SWITCH_OFF_THUMB_DARK = Color.FromArgb(255, 190, 190, 190);
        private static readonly Color SWITCH_OFF_TRACK_LIGHT = Color.FromArgb(100, 0, 0, 0);
        private static readonly Color SWITCH_OFF_TRACK_DARK = Color.FromArgb(100, 255, 255, 255);
        private static readonly Color SWITCH_OFF_DISABLED_THUMB_LIGHT = Color.FromArgb(255, 230, 230, 230);
        private static readonly Color SWITCH_OFF_DISABLED_THUMB_DARK = Color.FromArgb(255, 150, 150, 150);

        // Generic back colors - for user controls
        private static readonly Color BACKGROUND_LIGHT = Color.FromArgb(255, 255, 255, 255);
        private static readonly Brush BACKGROUND_LIGHT_BRUSH = new SolidBrush(BACKGROUND_LIGHT);
        private static readonly Color BACKGROUND_DARK = Color.FromArgb(255, 80, 80, 80);
        private static readonly Brush BACKGROUND_DARK_BRUSH = new SolidBrush(BACKGROUND_DARK);
        private static readonly Color BACKGROUND_ALTERNATIVE_LIGHT = Color.FromArgb(10, 0, 0, 0);
        private static readonly Brush BACKGROUND_ALTERNATIVE_LIGHT_BRUSH = new SolidBrush(BACKGROUND_ALTERNATIVE_LIGHT);
        private static readonly Color BACKGROUND_ALTERNATIVE_DARK = Color.FromArgb(10, 255, 255, 255);
        private static readonly Brush BACKGROUND_ALTERNATIVE_DARK_BRUSH = new SolidBrush(BACKGROUND_ALTERNATIVE_DARK);
        private static readonly Color BACKGROUND_HOVER_LIGHT = Color.FromArgb(50, 0, 0, 0);
        private static readonly Brush BACKGROUND_HOVER_LIGHT_BRUSH = new SolidBrush(BACKGROUND_HOVER_LIGHT);
        private static readonly Color BACKGROUND_HOVER_DARK = Color.FromArgb(50, 255, 255, 255);
        private static readonly Brush BACKGROUND_HOVER_DARK_BRUSH = new SolidBrush(BACKGROUND_HOVER_DARK);
        private static readonly Color BACKGROUND_HOVER_RED = Color.FromArgb(255, 255, 0, 0);
        private static readonly Brush BACKGROUND_HOVER_RED_BRUSH = new SolidBrush(BACKGROUND_HOVER_RED);
        private static readonly Color BACKGROUND_DOWN_RED = Color.FromArgb(255, 255, 84, 54);
        private static readonly Brush BACKGROUND_DOWN_RED_BRUSH = new SolidBrush(BACKGROUND_DOWN_RED);
        private static readonly Color BACKGROUND_FOCUS_LIGHT = Color.FromArgb(30, 0, 0, 0);
        private static readonly Brush BACKGROUND_FOCUS_LIGHT_BRUSH = new SolidBrush(BACKGROUND_FOCUS_LIGHT);
        private static readonly Color BACKGROUND_FOCUS_DARK = Color.FromArgb(30, 255, 255, 255);
        private static readonly Brush BACKGROUND_FOCUS_DARK_BRUSH = new SolidBrush(BACKGROUND_FOCUS_DARK);
        private static readonly Color BACKGROUND_DISABLED_LIGHT = Color.FromArgb(25, 0, 0, 0);
        private static readonly Brush BACKGROUND_DISABLED_LIGHT_BRUSH = new SolidBrush(BACKGROUND_DISABLED_LIGHT);
        private static readonly Color BACKGROUND_DISABLED_DARK = Color.FromArgb(25, 255, 255, 255);
        private static readonly Brush BACKGROUND_DISABLED_DARK_BRUSH = new SolidBrush(BACKGROUND_DISABLED_DARK);

        //Expansion Panel colors
        private static readonly Color EXPANSIONPANEL_FOCUS_LIGHT = Color.FromArgb(255, 242, 242, 242);
        private static readonly Brush EXPANSIONPANEL_FOCUS_LIGHT_BRUSH = new SolidBrush(EXPANSIONPANEL_FOCUS_LIGHT);
        private static readonly Color EXPANSIONPANEL_FOCUS_DARK = Color.FromArgb(255, 50, 50, 50);
        private static readonly Brush EXPANSIONPANEL_FOCUS_DARK_BRUSH = new SolidBrush(EXPANSIONPANEL_FOCUS_DARK);

        // Backdrop colors - for containers, like forms or panels
        private static readonly Color BACKDROP_LIGHT = Color.FromArgb(255, 242, 242, 242);
        private static readonly Brush BACKDROP_LIGHT_BRUSH = new SolidBrush(BACKGROUND_LIGHT);
        private static readonly Color BACKDROP_DARK = Color.FromArgb(255, 50, 50, 50);
        private static readonly Brush BACKDROP_DARK_BRUSH = new SolidBrush(BACKGROUND_DARK);

        //Other colors
        private static readonly Color CARD_BLACK = Color.FromArgb(255, 42, 42, 42);
        private static readonly Color CARD_WHITE = Color.White;
        #endregion
        #region Properties - Using these makes handling the dark theme switching easier
        // Getters - Using these makes handling the dark theme switching easier
        // Text
        public Color TextHighEmphasisColor => Theme == Themes.Light ? TEXT_HIGH_EMPHASIS_DARK : TEXT_HIGH_EMPHASIS_LIGHT;
        public Brush TextHighEmphasisBrush => Theme == Themes.Light ? TEXT_HIGH_EMPHASIS_DARK_BRUSH : TEXT_HIGH_EMPHASIS_LIGHT_BRUSH;
        public Color TextHighEmphasisNoAlphaColor => Theme == Themes.Light ? TEXT_HIGH_EMPHASIS_DARK_NOALPHA : TEXT_HIGH_EMPHASIS_LIGHT_NOALPHA;
        public Brush TextHighEmphasisNoAlphaBrush => Theme == Themes.Light ? TEXT_HIGH_EMPHASIS_DARK_NOALPHA_BRUSH : TEXT_HIGH_EMPHASIS_LIGHT_NOALPHA_BRUSH;
        public Color TextMediumEmphasisColor => Theme == Themes.Light ? TEXT_MEDIUM_EMPHASIS_DARK : TEXT_MEDIUM_EMPHASIS_LIGHT;
        public Brush TextMediumEmphasisBrush => Theme == Themes.Light ? TEXT_MEDIUM_EMPHASIS_DARK_BRUSH : TEXT_MEDIUM_EMPHASIS_LIGHT_BRUSH;
        public Color TextDisabledOrHintColor => Theme == Themes.Light ? TEXT_DISABLED_OR_HINT_DARK : TEXT_DISABLED_OR_HINT_LIGHT;
        public Brush TextDisabledOrHintBrush => Theme == Themes.Light ? TEXT_DISABLED_OR_HINT_DARK_BRUSH : TEXT_DISABLED_OR_HINT_LIGHT_BRUSH;

        // Divider
        public Color DividersColor => Theme == Themes.Light ? DIVIDERS_DARK : DIVIDERS_LIGHT;
        public Brush DividersBrush => Theme == Themes.Light ? DIVIDERS_DARK_BRUSH : DIVIDERS_LIGHT_BRUSH;
        public Color DividersAlternativeColor => Theme == Themes.Light ? DIVIDERS_ALTERNATIVE_DARK : DIVIDERS_ALTERNATIVE_LIGHT;
        public Brush DividersAlternativeBrush => Theme == Themes.Light ? DIVIDERS_ALTERNATIVE_DARK_BRUSH : DIVIDERS_ALTERNATIVE_LIGHT_BRUSH;

        // Checkbox / Radio / Switch
        public Color CheckboxOffColor => Theme == Themes.Light ? CHECKBOX_OFF_LIGHT : CHECKBOX_OFF_DARK;
        public Brush CheckboxOffBrush => Theme == Themes.Light ? CHECKBOX_OFF_LIGHT_BRUSH : CHECKBOX_OFF_DARK_BRUSH;
        public Color CheckBoxOffDisabledColor => Theme == Themes.Light ? CHECKBOX_OFF_DISABLED_LIGHT : CHECKBOX_OFF_DISABLED_DARK;
        public Brush CheckBoxOffDisabledBrush => Theme == Themes.Light ? CHECKBOX_OFF_DISABLED_LIGHT_BRUSH : CHECKBOX_OFF_DISABLED_DARK_BRUSH;

        // Switch
        public Color SwitchOffColor => Theme == Themes.Light ? CHECKBOX_OFF_DARK : CHECKBOX_OFF_LIGHT; // yes, I re-use the checkbox color, sue me
        public Color SwitchOffThumbColor => Theme == Themes.Light ? SWITCH_OFF_THUMB_LIGHT : SWITCH_OFF_THUMB_DARK;
        public Color SwitchOffTrackColor => Theme == Themes.Light ? SWITCH_OFF_TRACK_LIGHT : SWITCH_OFF_TRACK_DARK;
        public Color SwitchOffDisabledThumbColor => Theme == Themes.Light ? SWITCH_OFF_DISABLED_THUMB_LIGHT : SWITCH_OFF_DISABLED_THUMB_DARK;

        // Control Back colors
        public Color BackgroundColor => Theme == Themes.Light ? BACKGROUND_LIGHT : BACKGROUND_DARK;
        public Brush BackgroundBrush => Theme == Themes.Light ? BACKGROUND_LIGHT_BRUSH : BACKGROUND_DARK_BRUSH;
        public Color BackgroundAlternativeColor => Theme == Themes.Light ? BACKGROUND_ALTERNATIVE_LIGHT : BACKGROUND_ALTERNATIVE_DARK;
        public Brush BackgroundAlternativeBrush => Theme == Themes.Light ? BACKGROUND_ALTERNATIVE_LIGHT_BRUSH : BACKGROUND_ALTERNATIVE_DARK_BRUSH;
        public Color BackgroundDisabledColor => Theme == Themes.Light ? BACKGROUND_DISABLED_LIGHT : BACKGROUND_DISABLED_DARK;
        public Brush BackgroundDisabledBrush => Theme == Themes.Light ? BACKGROUND_DISABLED_LIGHT_BRUSH : BACKGROUND_DISABLED_DARK_BRUSH;
        public Color BackgroundHoverColor => Theme == Themes.Light ? BACKGROUND_HOVER_LIGHT : BACKGROUND_HOVER_DARK;
        public Brush BackgroundHoverBrush => Theme == Themes.Light ? BACKGROUND_HOVER_LIGHT_BRUSH : BACKGROUND_HOVER_DARK_BRUSH;
        public Color BackgroundHoverRedColor => Theme == Themes.Light ? BACKGROUND_HOVER_RED : BACKGROUND_HOVER_RED;
        public Brush BackgroundHoverRedBrush => Theme == Themes.Light ? BACKGROUND_HOVER_RED_BRUSH : BACKGROUND_HOVER_RED_BRUSH;
        public Brush BackgroundDownRedBrush => Theme == Themes.Light ? BACKGROUND_DOWN_RED_BRUSH : BACKGROUND_DOWN_RED_BRUSH;
        public Color BackgroundFocusColor => Theme == Themes.Light ? BACKGROUND_FOCUS_LIGHT : BACKGROUND_FOCUS_DARK;
        public Brush BackgroundFocusBrush => Theme == Themes.Light ? BACKGROUND_FOCUS_LIGHT_BRUSH : BACKGROUND_FOCUS_DARK_BRUSH;


        // Other color
        public Color CardsColor => Theme == Themes.Light ? CARD_WHITE : CARD_BLACK;

        // Expansion Panel color/brush
        public Brush ExpansionPanelFocusBrush => Theme == Themes.Light ? EXPANSIONPANEL_FOCUS_LIGHT_BRUSH : EXPANSIONPANEL_FOCUS_DARK_BRUSH;

        // SnackBar
        public Color SnackBarTextHighEmphasisColor => Theme != Themes.Light ? TEXT_HIGH_EMPHASIS_DARK : TEXT_HIGH_EMPHASIS_LIGHT;
        public Color SnackBarBackgroundColor => Theme != Themes.Light ? BACKGROUND_LIGHT : BACKGROUND_DARK;
        public Color SnackBarTextButtonNoAccentTextColor => Theme != Themes.Light ? ColorScheme.PrimaryColor : ColorScheme.LightPrimaryColor;

        // Backdrop color
        public Color BackdropColor => Theme == Themes.Light ? BACKDROP_LIGHT : BACKDROP_DARK;
        public Brush BackdropBrush => Theme == Themes.Light ? BACKDROP_LIGHT_BRUSH : BACKDROP_DARK_BRUSH;

        #endregion

        // Font Handling
        public enum FontType
        {
            H1,
            H2,
            H3,
            H4,
            H5,
            H6,
            Subtitle1,
            Subtitle2,
            SubtleEmphasis,
            Body1,
            Body2,
            Button,
            Caption,
            Overline,
            Custom,
            MenuStrip
        }

        public enum CustomFontFamily
        {
            Roboto,
            Roboto_Thin,
            Roboto_Medium,
            Roboto_Light,
            //Roboto_Bold,
            Roboto_Black,
            Material_Icons,
            Material_Icons_Round,
            Material_Icons_Outlined,
            Material_Icons_Sharp
        }

        public Font GetFont(CustomFontFamily family)
        {
            if(_robotoFontFamilies.ContainsKey(family.ToString()))
            {
                return new Font(_robotoFontFamilies[family.ToString()], 12.25f, FontStyle.Regular, GraphicsUnit.Point);
            }

            return new Font(_robotoFontFamilies["Roboto"], 12.25f, FontStyle.Regular, GraphicsUnit.Point);
        }

        public Font GetFontByType(FontType type, int dpi = 96)
        {
            var f = dpi / 96f;

            switch (type)
            {
                case FontType.H1:
                    return new Font(_robotoFontFamilies["Roboto_Light"], f * 96f, FontStyle.Regular, GraphicsUnit.Pixel);

                case FontType.H2:
                    return new Font(_robotoFontFamilies["Roboto_Light"], f * 60f, FontStyle.Regular, GraphicsUnit.Pixel);

                case FontType.H3:
                    return new Font(_robotoFontFamilies["Roboto"], f * 48f, FontStyle.Bold, GraphicsUnit.Pixel);

                case FontType.H4:
                    return new Font(_robotoFontFamilies["Roboto"], f * 34f, FontStyle.Bold, GraphicsUnit.Pixel);

                case FontType.H5:
                    return new Font(_robotoFontFamilies["Roboto"], f * 24f, FontStyle.Bold, GraphicsUnit.Pixel);

                case FontType.H6:
                    return new Font(_robotoFontFamilies["Roboto_Medium"], f * 20f, FontStyle.Bold, GraphicsUnit.Pixel);

                case FontType.Subtitle1:
                    return new Font(_robotoFontFamilies["Roboto"], f * 16f, FontStyle.Regular, GraphicsUnit.Pixel);

                case FontType.Subtitle2:
                    return new Font(_robotoFontFamilies["Roboto_Medium"], f * 14f, FontStyle.Bold, GraphicsUnit.Pixel);

                case FontType.SubtleEmphasis:
                    return new Font(_robotoFontFamilies["Roboto"], f * 12f, FontStyle.Italic, GraphicsUnit.Pixel);

                case FontType.Body1:
                    return new Font(_robotoFontFamilies["Roboto"], f * 8.25f, FontStyle.Regular, GraphicsUnit.Point);

                case FontType.Body2:
                    return new Font(_robotoFontFamilies["Roboto"],   f * 12f, FontStyle.Regular, GraphicsUnit.Pixel);

                case FontType.Button:
                    return new Font(_robotoFontFamilies["Roboto"], f * 8.25f, FontStyle.Bold, GraphicsUnit.Pixel);

                case FontType.Caption:
                    return new Font(_robotoFontFamilies["Roboto"], f * 12f, FontStyle.Regular, GraphicsUnit.Pixel);

                case FontType.Overline:
                    return new Font(_robotoFontFamilies["Roboto"], f * 10f, FontStyle.Regular, GraphicsUnit.Pixel);
            }
            return new Font(_robotoFontFamilies["Roboto"], f * 14f, FontStyle.Regular, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Get the font by size - used for textbox label animation, try to not use this for anything else
        /// </summary>
        /// <param name="size">font size, ranges from 12 up to 16</param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public IntPtr GetTextBoxFontBySize(int size, int dpi = 96)
        {
            var key = "textBox" + Math.Min(16, Math.Max(12, size)).ToString() + "-" + dpi.ToString();
            if (_logicalFonts.TryGetValue(key, out var font))
                return font;
            
            IntPtr newFont;
            
            if (size > 13)
            {
                newFont = CreateLogicalFont("Roboto", size, NativeTextRenderer.logFontWeight.FW_NORMAL, 0, dpi);
            }
            else
            {
                newFont = CreateLogicalFont("Roboto Medium", size, NativeTextRenderer.logFontWeight.FW_MEDIUM, 0, dpi);
            }

            _logicalFonts[key] = newFont;
            return newFont;
        }

        /// <summary>
        /// Gets a Material Skin Logical Roboto Font given a standard material font type
        /// </summary>
        /// <param name="type">material design font type</param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public IntPtr GetLogFontByType(FontType type, int dpi = 96)
        {
            var key = Enum.GetName(typeof(FontType), type) + "-" + dpi.ToString();
            if (_logicalFonts.TryGetValue(key, out var font))
                return font;
            var newFont = CreateLogicalFontByType(type, dpi);
            _logicalFonts[key] = newFont;
            return newFont;
        }

        // Font stuff
        private readonly Dictionary<string, IntPtr> _logicalFonts;

        private readonly Dictionary<string, FontFamily> _robotoFontFamilies;

        private readonly PrivateFontCollection _privateFontCollection = new();

        private void AddFont(byte[] fontData)
        {
            // Add font to system table in memory
            var dataLength = fontData.Length;

            var ptrFont = Marshal.AllocCoTaskMem(dataLength);
            Marshal.Copy(fontData, 0, ptrFont, dataLength);

            // GDI Font
            NativeTextRenderer.AddFontMemResourceEx(fontData, dataLength, IntPtr.Zero, out _);

            // GDI+ Font
            _privateFontCollection.AddMemoryFont(ptrFont, dataLength);
        }

        private static IntPtr CreateLogicalFont(string fontName, int size, NativeTextRenderer.logFontWeight weight, byte lfItalic = 0, int dpi = 96)
        {
            // Logical font:
            var lFont = new NativeTextRenderer.LogFont
            {
                lfFaceName = fontName,
                lfHeight = (int)Math.Round(-size * dpi / 96f),
                lfWeight = (int)weight,
                lfItalic = lfItalic
            };
            return NativeTextRenderer.CreateFontIndirect(lFont);
        }

        private static IntPtr CreateLogicalFontByType(FontType type, int dpi = 96)
        {
            switch (type) { 
                case FontType.H1:
                    return CreateLogicalFont("Roboto Light", 96, NativeTextRenderer.logFontWeight.FW_LIGHT, 0, dpi);
                case FontType.H2:
                    return CreateLogicalFont("Roboto Light", 60, NativeTextRenderer.logFontWeight.FW_LIGHT, 0, dpi);
                case FontType.H3:
                    return CreateLogicalFont("Roboto", 48, NativeTextRenderer.logFontWeight.FW_REGULAR, 0, dpi);
                case FontType.H4:
                    return CreateLogicalFont("Roboto", 34, NativeTextRenderer.logFontWeight.FW_REGULAR, 0, dpi);
                case FontType.H5:
                    return CreateLogicalFont("Roboto", 24, NativeTextRenderer.logFontWeight.FW_REGULAR, 0, dpi);
                case FontType.H6:
                    return CreateLogicalFont("Roboto Medium", 20, NativeTextRenderer.logFontWeight.FW_MEDIUM, 0, dpi);
                case FontType.Subtitle1:
                    return CreateLogicalFont("Roboto", 16, NativeTextRenderer.logFontWeight.FW_REGULAR, 0, dpi);
                case FontType.Subtitle2:
                    return CreateLogicalFont("Roboto Medium", 14, NativeTextRenderer.logFontWeight.FW_MEDIUM, 0, dpi);
                case FontType.SubtleEmphasis:
                    return CreateLogicalFont("Roboto", 12, NativeTextRenderer.logFontWeight.FW_NORMAL, 1, dpi);
                case FontType.Body1:
                    return CreateLogicalFont("Roboto", 16, NativeTextRenderer.logFontWeight.FW_REGULAR, 0, dpi);
                case FontType.Body2:
                    return CreateLogicalFont("Roboto", 14, NativeTextRenderer.logFontWeight.FW_REGULAR, 0, dpi);
                case FontType.Button:
                    return CreateLogicalFont("Roboto Medium", 14, NativeTextRenderer.logFontWeight.FW_MEDIUM, 0, dpi);
                case FontType.Caption:
                    return CreateLogicalFont("Roboto", 12, NativeTextRenderer.logFontWeight.FW_REGULAR, 0, dpi);
                case FontType.Overline:
                    return CreateLogicalFont("Roboto", 10, NativeTextRenderer.logFontWeight.FW_REGULAR, 0, dpi);
            }

            return IntPtr.Zero;
        }

        public FontFamily GetFontFamily(string fontName)
        {
            return _robotoFontFamilies[fontName];
        }

        /// <summary>
        /// font size in pt
        /// </summary>
        /// <param name="size"></param>
        public void SetDefaultFontSize(float size) {
            DefaultFont = new Font(DefaultFont.FontFamily, size, DefaultFont.Style, GraphicsUnit.Point);
        }

        // Dyanmic Themes
        public void AddFormToManage(MaterialForm materialForm)
        {
            _formsToManage.Add(materialForm);
            UpdateBackgrounds();

            // Set background on newly added controls
            materialForm.ControlAdded += (sender, e) =>
            {
                UpdateControlBackColor(e.Control, BackdropColor);
            };
        }

        public void RemoveFormToManage(MaterialForm materialForm)
        {
            _formsToManage.Remove(materialForm);
        }

        private void UpdateBackgrounds()
        {
            var newBackColor = BackdropColor;
            foreach (var materialForm in _formsToManage)
            {
                materialForm.BackColor = newBackColor;
                UpdateControlBackColor(materialForm, newBackColor);
            }
        }

        private void UpdateControlBackColor(Control controlToUpdate, Color newBackColor)
        {
            // No control
            if (controlToUpdate == null) return;

            // Control's Context menu
            if (controlToUpdate.ContextMenuStrip != null) UpdateToolStrip(controlToUpdate.ContextMenuStrip, newBackColor);

            // Material Tabcontrol pages
            if (controlToUpdate is TabPage page)
            {
                page.BackColor = newBackColor;
            }

            // Material Divider
            else if (controlToUpdate is MaterialDivider)
            {
                controlToUpdate.BackColor = DividersColor;
            }

            // Other Material Skin control
            else if (controlToUpdate.IsMaterialControl() && !(controlToUpdate.HasProperty("UseCustomColor") && ((MaterialLabel)controlToUpdate).UseCustomColor))
            {
                if(controlToUpdate.HasProperty("_UseCustomBackgroundColor") && !((MaterialCard)controlToUpdate)._UseCustomBackgroundColor)
                {
                    controlToUpdate.BackColor = newBackColor;
                }

                controlToUpdate.ForeColor = TextHighEmphasisColor;
            }

            // Other Generic control not part of material skin
            else if (EnforceBackcolorOnAllComponents && controlToUpdate.HasProperty("BackColor") && !controlToUpdate.IsMaterialControl() && controlToUpdate.Parent != null)
            {
                if (!controlToUpdate.Name.StartsWith("pan"))
                {
                    controlToUpdate.BackColor = controlToUpdate.Parent.BackColor;
                    controlToUpdate.ForeColor = TextHighEmphasisColor;
                    controlToUpdate.Font = DefaultFont;

                    if(controlToUpdate is DataGridView view)
                    {
                        view.DefaultCellStyle.BackColor = BackgroundColor;
                        view.DefaultCellStyle.ForeColor = TextHighEmphasisColor;
                        view.DefaultCellStyle.Font = DefaultFont;
                        view.RowHeadersDefaultCellStyle.BackColor = BackgroundColor;
                        view.RowHeadersDefaultCellStyle.ForeColor = TextHighEmphasisColor;
                        view.ColumnHeadersDefaultCellStyle.Font = DefaultFont;
                        view.ColumnHeadersDefaultCellStyle.BackColor = BackgroundColor;
                        view.ColumnHeadersDefaultCellStyle.ForeColor = TextHighEmphasisColor;
                        view.EnableHeadersVisualStyles = true;
                        if (view.Parent != null) view.BackgroundColor = view.Parent.BackColor;
                    }
                }
            }

            // Recursive call to control's children
            foreach (Control control in controlToUpdate.Controls)
            {
                UpdateControlBackColor(control, newBackColor);
            }
        }

        private static void UpdateToolStrip(ToolStrip toolStrip, Color newBackColor)
        {
            if (toolStrip == null)
            {
                return;
            }

            toolStrip.BackColor = newBackColor;
            foreach (ToolStripItem control in toolStrip.Items)
            {
                control.BackColor = newBackColor;
                if (control is MaterialToolStripMenuItem && (control as MaterialToolStripMenuItem).HasDropDown)
                {
                    //recursive call
                    UpdateToolStrip((control as MaterialToolStripMenuItem).DropDown, newBackColor);
                }
            }
        }
    }
}
