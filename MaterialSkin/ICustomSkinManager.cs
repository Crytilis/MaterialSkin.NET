namespace MaterialSkin
{
    /// <summary>
    /// Allows a control to specify a custom MaterialSkinManager instance.
    /// This enables per-control theming, offering the flexibility to diverge
    /// from the global theme settings defined by the singleton MaterialSkinManager.Instance.
    /// </summary>
    public interface ICustomSkinManager
    {
        /// <summary>
        /// Gets or sets a custom MaterialSkinManager instance for the control.
        /// Setting this property allows the control to apply a specific theme
        /// or styling that may differ from the global application theme.
        /// </summary>
        MaterialSkinManager CustomSkinManager { get; set; }
    }
}
