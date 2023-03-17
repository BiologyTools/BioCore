using Newtonsoft.Json;
using WindowsInput;

namespace Bio
{
    public partial class FunctionForm : Form
    {
        public static InputSimulator input = new InputSimulator();
        private Function func;
        /* A property. */
        public Function Func
        {
            get
            {
                return func;
            }
            set
            {
                func = value;
            }
        }
        /* Initializing the form. */
        public FunctionForm(Function func)
        {
            InitializeComponent();
            Func = func;
            Init();
        }
        /* Initializing the form. */
        public FunctionForm(Function func, string name)
        {
            InitializeComponent();
            Func = func;
            func.Name = name;
            Init();
        }
        /// It initializes the form with the values of the function
        private void Init()
        {
            int ind = 0;
            int sel = 0;

            textBox.Text = func.Script;

            funcsBox.Items.Clear();
            foreach (Function f in Function.Functions.Values)
            {
                funcsBox.Items.Add(f);
            }

            nameBox.Text = func.Name;
            //We add button states to buttonState box.
            stateBox.Items.Clear();
            foreach (Function.ButtonState val in Enum.GetValues(typeof(Function.ButtonState)))
            {
                stateBox.Items.Add(val);
            }
            stateBox.SelectedItem = func.State;

            //We add button states to buttonState box.
            ind = 0;
            keysBox.Items.Clear();
            foreach (VirtualKeyCode val in Enum.GetValues(typeof(VirtualKeyCode)))
            {
                keysBox.Items.Add(val);
                if (val == func.Key)
                    keysBox.SelectedIndex = ind;
                ind++;
            }
            //We add the modifiers to modifierBox
            modifierBox.Items.Clear();
            modifierBox.Items.Add(VirtualKeyCode.NONAME);
            modifierBox.Items.Add(VirtualKeyCode.RCONTROL);
            modifierBox.Items.Add(VirtualKeyCode.LCONTROL);
            modifierBox.Items.Add(VirtualKeyCode.RSHIFT);
            modifierBox.Items.Add(VirtualKeyCode.LSHIFT);
            modifierBox.Items.Add(VirtualKeyCode.MENU);
            modifierBox.Items.Add(VirtualKeyCode.RWIN);
            modifierBox.Items.Add(VirtualKeyCode.LWIN);
            modifierBox.SelectedItem = func.Modifier;
            ind = 0;
            valBox.Value = (decimal)func.Value;
            menuPath.Text = func.MenuPath;
            contextMenuPath.Text = func.ContextPath;
        }
        /// It updates the textboxes and comboboxes with the values of the function
        private void UpdateItems()
        {
            textBox.Text = func.Script;
            nameBox.Text = func.Name;
            stateBox.SelectedItem = func.State;
            keysBox.SelectedItem = func.Key;
            modifierBox.SelectedItem = func.Modifier;
            valBox.Value = (decimal)func.Value;
            if (func.FuncType == Function.FunctionType.ImageJ)
                imageJRadioBut.Checked = true;
            else
                imageJRadioBut.Checked = false;
            menuPath.Text = func.MenuPath;
            contextMenuPath.Text = func.ContextPath;
        }
        /// When the user selects a key from the dropdown menu, the key is set to the selected key
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void keysBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Func.Key = (VirtualKeyCode)keysBox.SelectedItem;
            Func.FuncType = Function.FunctionType.Key;
        }
        /// When the user selects a modifier key from the dropdown menu, the function's modifier is set
        /// to the selected key
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void modifierBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Func.Modifier = (VirtualKeyCode)modifierBox.SelectedItem;
            Func.FuncType = Function.FunctionType.Key;
        }
        /// When the user selects a new item in the stateBox, the state of the function is set to the
        /// selected item and the function type is set to key
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void stateBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Func.State = (Function.ButtonState)stateBox.SelectedItem;
            Func.FuncType = Function.FunctionType.Key;
        }
        /// It saves the function
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void applyButton_Click(object sender, EventArgs e)
        {
            func.Name = nameBox.Text;
            this.DialogResult = DialogResult.OK;
            if (!Function.Functions.ContainsKey(func.Name))
            {
                Function.Functions.Add(func.Name, func);
            }
            else
            {
                Function.Functions[func.Name] = func;
            }
            if (func.MenuPath != null && func.MenuPath != "")
            {
                if (func.MenuPath.EndsWith("/"))
                    func.MenuPath = func.MenuPath.TrimEnd('/');
                if (func.MenuPath.EndsWith(func.Name) && func.MenuPath.Contains("/"))
                    func.MenuPath.Remove(func.MenuPath.IndexOf('/'), func.MenuPath.Length - func.MenuPath.IndexOf('/'));
                App.AddMenu(func.MenuPath, func);
            }
            if (func.ContextPath != null && func.ContextPath != "")
            {
                if (func.ContextPath.EndsWith("/"))
                    func.ContextPath = func.ContextPath.TrimEnd('/');
                if (func.ContextPath.EndsWith(func.Name) && func.ContextPath.Contains("/"))
                    func.ContextPath.Remove(func.ContextPath.IndexOf('/'), func.ContextPath.Length - func.ContextPath.IndexOf('/'));
                App.AddContextMenu(func.ContextPath, func);
            }
            func.Save();
            Init();
        }

        /// If the user selects a file, set the file name and the function type to ImageJ
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        /// 
        /// @return The file name of the macro file.
        private void setMacroFileBut_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            Func.File = openFileDialog.FileName;
            Func.FuncType = Function.FunctionType.ImageJ;
        }
        /// If the user selects a file, set the file name and function type
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        /// 
        /// @return The file name of the file that was selected.
        private void setScriptFileBut_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() != DialogResult.OK)
                return;
            Func.File = openFileDialog.FileName;
            Func.FuncType = Function.FunctionType.Script;
        }
        /// When the value of the numeric up/down control changes, the value of the function is set to
        /// the value of the numeric up/down control
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void valBox_ValueChanged(object sender, EventArgs e)
        {
            Func.Value = (double)valBox.Value;
        }
        /// If the imageJRadioBut is checked, then the PerformFunction function is called with the
        /// argument true. Otherwise, the PerformFunction function is called with the argument false
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void performBut_Click(object sender, EventArgs e)
        {
            MessageBox.Show(func.PerformFunction(imageJRadioBut.Checked).ToString());
        }

        /// The function cancelBut_Click is a private function that takes two parameters, sender and e,
        /// and returns void
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void cancelBut_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
        /// The text in the textbox is set to the script property of the function object
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            func.Script = textBox.Text;
            if (bioRadioBut.Checked)
                func.FuncType = Function.FunctionType.Script;
            else
                func.FuncType = Function.FunctionType.ImageJ;
        }

        /// If the radio button is checked, set the function type to script
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs e
        private void bioRadioBut_CheckedChanged(object sender, EventArgs e)
        {
            if (bioRadioBut.Checked)
                func.FuncType = Function.FunctionType.Script;
        }

       /// If the imageJRadioBut is checked, then set the function type to imageJ
       /// 
       /// @param sender The object that raised the event.
       /// @param EventArgs e
        private void imageJRadioBut_CheckedChanged(object sender, EventArgs e)
        {
            if (imageJRadioBut.Checked)
                func.FuncType = Function.FunctionType.ImageJ;
        }

        /// The function is called when the text in the menuPath textbox is changed
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void menuPath_TextChanged(object sender, EventArgs e)
        {
            func.MenuPath = menuPath.Text;
        }

/// This function is called when the form is activated.
/// 
/// @param sender The object that raised the event.
/// @param EventArgs The EventArgs class is the base class for classes containing event data.
        private void FunctionForm_Activated(object sender, EventArgs e)
        {

        }

/// If the user selects a function from the dropdown list, then update the list of items
/// 
/// @param sender The object that raised the event.
/// @param EventArgs The event arguments.
/// 
/// @return The function that is selected in the combo box.
        private void funcsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (funcsBox.SelectedIndex == -1)
                return;
            func = (Function)funcsBox.SelectedItem;
            UpdateItems();
        }

        /// This function is called when the form is closing
        /// 
        /// @param sender The object that raised the event.
        /// @param FormClosingEventArgs 
        private void FunctionForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        /// The function above is called when the text in the textbox is changed. It sets the
        /// ContextPath variable in the func class to the text in the textbox
        /// 
        /// @param sender The object that raised the event.
        /// @param EventArgs The event arguments.
        private void contextMenuPath_TextChanged(object sender, EventArgs e)
        {
            func.ContextPath = contextMenuPath.Text;
        }
    }

    public class Function
    {
        public static Dictionary<string, Function> Functions = new Dictionary<string, Function>();
        /* Defining an enum. */
        public enum FunctionType
        {
            Key,
            Microscope,
            Objective,
            StoreCoordinate,
            NextCoordinate,
            PreviousCoordinate,
            NextSnapCoordinate,
            PreviousSnapCoordinate,
            Recording,
            Property,
            ImageJ,
            Script,
            None
        }
        public enum ButtonState
        {
            Pressed,
            Released
        }

        private ButtonState buttonState = ButtonState.Pressed;
        public ButtonState State
        {
            get
            {
                return buttonState;
            }
            set
            {
                buttonState = value;
            }
        }

        private VirtualKeyCode key;
        public VirtualKeyCode Key
        {
            get
            {
                return key;
            }
            set
            {
                key = value;
                FuncType = FunctionType.Key;
            }
        }

        private VirtualKeyCode modifier = VirtualKeyCode.NONAME;
        public VirtualKeyCode Modifier
        {
            get
            {
                return modifier;
            }
            set
            {
                FuncType = FunctionType.Key;
                modifier = value;
            }
        }

        private FunctionType functionType;
        public FunctionType FuncType
        {
            get
            {
                return functionType;
            }
            set
            {
                functionType = value;
            }
        }

        private string file;
        public string File
        {
            get { return file; }
            set { file = value; }
        }

        private string script;
        public string Script
        {
            get { return script; }
            set
            {
                script = value;
            }
        }

        private string name;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
        private string menuPath;
        public string MenuPath
        {
            get
            {
                return menuPath;
            }
            set
            {
                menuPath = value;
            }
        }
        private string contextPath;
        public string ContextPath
        {
            get
            {
                return contextPath;
            }
            set
            {
                contextPath = value;
            }
        }

        private double val;
        public double Value
        {
            get
            {
                return val;
            }
            set
            {
                val = value;
            }
        }
        private string microscope;
        public string Microscope
        {
            get
            {
                return microscope;
            }
            set
            {
                microscope = value;
            }
        }
        public override string ToString()
        {
            return name + ", " + MenuPath;
        }
        /// It takes a string, and if it's empty, it returns a new Function object. Otherwise, it
        /// returns a new Function object that is the deserialized version of the string
        /// 
        /// @param s The string to parse
        /// 
        /// @return A function object.
        public static Function Parse(string s)
        {
            if (s == "")
                return new Function();
            return JsonConvert.DeserializeObject<Function>(s);
        }

        /// "Serialize" is a function that takes an object and returns a string
        /// 
        /// @return The object is being serialized into a JSON string.
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static InputSimulator input = new InputSimulator();
        /// It runs a script or a keystroke
        /// 
        /// @param imagej boolean, whether to run the script as an ImageJ script or a C# script
        /// 
        /// @return The return value is the result of the function.
        public object PerformFunction(bool imagej)
        {
            if (FuncType == FunctionType.Key && Script != "")
                if (imagej)
                {
                    FuncType = FunctionType.ImageJ;
                }
                else
                    FuncType = FunctionType.Script;
            if (FuncType == Function.FunctionType.Script)
            {
                Scripting.RunString(script);
            }
            if (FuncType == Function.FunctionType.ImageJ)
            {
                ImageJ.RunOnImage(script, false, BioConsole.onTab, BioConsole.useBioformats);
            }

            if (FuncType == Function.FunctionType.Key)
            {
                if (Modifier != VirtualKeyCode.NONAME)
                {
                    input.Keyboard.ModifiedKeyStroke(Modifier, Key);
                }
                else
                {
                    input.Keyboard.KeyPress(Key);
                }
                return null;
            }
            return null;
        }
        /// It reads all the files in the Functions folder, parses them into Function objects, and adds
        /// them to the Functions dictionary
        public static void Initialize()
        {
            string st = Application.StartupPath + "/Functions";
            if (!Directory.Exists(st))
                Directory.CreateDirectory(st);
            string[] sts = Directory.GetFiles(st);
            for (int i = 0; i < sts.Length; i++)
            {
                string fs = System.IO.File.ReadAllText(sts[i]);
                Function f = Function.Parse(fs);
                if (!Functions.ContainsKey(f.Name))
                    Functions.Add(f.Name, f);
                App.AddMenu(f.MenuPath, f);
                App.AddContextMenu(f.ContextPath, f);
            }
        }
        /// It saves all the functions in the Functions dictionary to a file
        public void Save()
        {
            string st = Application.StartupPath;

            if (!Directory.Exists(st + "/Functions"))
            {
                Directory.CreateDirectory(st + "/Functions");
            }
            foreach (Function f in Functions.Values)
            {
                System.IO.File.WriteAllText(st + "/Functions/" + f.Name + ".func", f.Serialize());
            }
        }
    }

}
