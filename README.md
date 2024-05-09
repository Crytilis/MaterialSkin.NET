# MaterialSkin for .NET WinForms

Theming C# .NET WinForms to Google's Material Design Principles.

> This project state is: "Stasis"
>


## Current state of the MaterialSkin components

| Component                    | Supported | Disabled mode | Animated |
| ---------------------------- | :-------: | :-----------: | :------: |
| Backdrop                     |  **No**   |       -       |    -     |
| Banner                       |  **No**   |       -       |    -     |
| Buttons                      |    Yes    |      Yes      |   Yes    |
| Cards                        |    Yes    |      N/A      |   N/A    |
| Check Box                    |    Yes    |      Yes      |   Yes    |
| Check Box List               |    Yes    |      Yes      |   Yes    |
| Chips                        |  **No**   |       -       |    -     |
| Combobox                     |    Yes    |      Yes      |   Yes    |
| Context Menu                 |    Yes    |      Yes      |   Yes    |
| Date Picker                  |    Yes    |      N/A      |   N/A    |
| Color Picker                 |    Yes    |      N/A      |   N/A    |
| Dialog                       |    Yes    |      N/A      |  **No**  |
| Divider                      |    Yes    |      N/A      |   N/A    |
| Drawer                       |    Yes    |      N/A      |   Yes    |
| Expansion Panel              |    Yes    |      Yes      |  **No**  |
| Flexible Dialog (big)        |    Yes    |      Yes      |   N/A    |
| FAB - Floating Action Button |    Yes    |      Yes      |   Yes    |
| FlowLayoutPanel              |    Yes    |      N/A      |   N/A    |
| Label                        |    Yes    |      Yes      |   N/A    |
| ListBox                      |    Yes    |      Yes      |   N/A    |
| ListView                     |    Yes    |    **No**     |   N/A    |
| Panel                        |    Yes    |      N/A      |   N/A    |
| Progress Bar                 |  _Partial_  |    **No**     |  **No**  |
| Radio Button                 |    Yes    |      Yes      |   Yes    |
| Text field                   |    Yes    |      Yes      |   Yes    |
| Sliders                      |    Yes    |      Yes      |  **No**  |
| SnackBar                     |    Yes    |      N/A      |   Yes    |
| Switch                       |    Yes    |      Yes      |   Yes    |
| Tabs                         |    Yes    |      N/A      |   Yes    |
| Time Picker                  |  **No**   |       -       |    -     |
| Tooltips                     |  **No**   |       -       |    -     |

All supported components have a dark theme

---

> - This project was heavily updated by [@leocb](https://github.com/leocb/MaterialSkin)
> - Currently it's kept alive by [@orapps44](https://github.com/orapps44/MaterialSkin)
> - forked from [@donaldsteele](https://github.com/donaldsteele/MaterialSkin)
> - and he forked it from the original [@IgnaceMaes](https://github.com/IgnaceMaes/MaterialSkin)

## Contributors

Thank you to all the people who have already contributed to the MaterialSkin projects!

<a href="https://github.com/leocb/MaterialSkin/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=leocb/MaterialSkin" />
</a>


---

## Implementing MaterialSkin in your application

### 1. Add the library to your project

Clone the project from GitHub, then add the MaterialSkin.csproj to your own solution, then add it as a project reference on your project.
  
### 2. Add the MaterialSkin components to your ToolBox

Simply drag the MaterialSkin.dll file into your IDE's ToolBox and all the controls should be added there.

### 3. Inherit from MaterialForm

Open the code behind your Form you wish to skin. Make it inherit from MaterialForm rather than Form. Don't forget to put the library in your imports, so it can find the MaterialForm class!
  
#### C# (Form1.cs)

```cs
public partial class Form1 : MaterialForm
```
   
### 4. Initialize your colorscheme

Set your preferred colors & theme. Also add the form to the manager so it keeps updated if the color scheme or theme changes later on.

#### C# (Form1.cs)

```cs
public Form1()
{
    InitializeComponent();

    var materialSkinManager = MaterialSkinManager.Instance;
    materialSkinManager.AddFormToManage(this);
    materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
    materialSkinManager.ColorScheme = new ColorScheme(Primary.BlueGrey800, Primary.BlueGrey900, Primary.BlueGrey500, Accent.LightBlue200, TextShade.WHITE);
}
```

---

## Material Design in WPF

If you love .NET and Material Design, you should definitely check out [Material Design Xaml Toolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit) by ButchersBoy. It's a similar project but for WPF instead of WinForms.

---

## Images

*A simple demo interface with MaterialSkin components.*
![home](https://user-images.githubusercontent.com/77468294/134770847-0f20f37f-e3e7-4e15-b838-cf53b0b32c4e.png)

*The MaterialSkin Drawer (menu).*
![drawer](https://user-images.githubusercontent.com/77468294/119880600-b4570480-bf2c-11eb-9a83-e2d59b88bf22.png)

*Every MaterialSkin button variant - this is 1 control, 3 properties*
![buttons](https://user-images.githubusercontent.com/77468294/119880777-e1a3b280-bf2c-11eb-8042-45b767459b41.png)

*The MaterialSkin checkboxes, radio and Switch.*
![selection](https://user-images.githubusercontent.com/77468294/134740844-795cd759-e4dc-4592-b2c1-86896e61f66f.png)

*Material skin textfield*
![text](https://user-images.githubusercontent.com/77468294/134825517-23f517e7-0793-4c4c-bcb2-6c2c2fb36b4a.png)

*Table control*
![table](https://user-images.githubusercontent.com/8310271/66237915-a1931a80-e6cc-11e9-8e68-bc919f533366.png)

*Progress bar*
![progress bar](https://user-images.githubusercontent.com/77468294/119880969-131c7e00-bf2d-11eb-9ec6-b00e928e59ed.png)

*Cards*
![cards](https://user-images.githubusercontent.com/77468294/119881312-6f7f9d80-bf2d-11eb-93b8-e4dc58dc3a4e.png)

*List Box*
![listbox](https://user-images.githubusercontent.com/77468294/119881063-2891a800-bf2d-11eb-93d8-d0395dc1f19e.png)

*Expansion Panel*
![expansion](https://user-images.githubusercontent.com/77468294/119881153-419a5900-bf2d-11eb-95a2-ab29089acdd3.png)

*Label*
![label](https://user-images.githubusercontent.com/77468294/132769098-440841c8-07d2-4b9b-bff7-e525402525dd.png)

*MaterialSkin using a custom color scheme.*
![custom](https://user-images.githubusercontent.com/77468294/119881411-8e7e2f80-bf2d-11eb-9fa3-883eceabfadc.png)

*FlexibleMaterial Messagebox*
![messagebox](https://user-images.githubusercontent.com/8310271/66238105-25e59d80-e6cd-11e9-88c9-5a21ceae1a5a.png)
