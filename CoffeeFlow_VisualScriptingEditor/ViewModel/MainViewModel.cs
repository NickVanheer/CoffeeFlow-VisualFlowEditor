using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Win32;
using CoffeeFlow.Annotations;
using CoffeeFlow.Base;
using CoffeeFlow.Nodes;
using CoffeeFlow.Views;
using Roslyn.Compilers.CSharp;
using UnityFlow;
using Newtonsoft.Json;

namespace CoffeeFlow.ViewModel
{
    /**********************************************************************************************************
      *             Logic related to the main window, window logic, code parsing and node list. 
      * 
      *                                                      * Nick @ http://immersivenick.wordpress.com 
      *                                                      * Free for non-commercial use
      * *********************************************************************************************************/
    public class MainViewModel : ViewModelBase
    {
        private static MainViewModel instance;
        public static MainViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new System.ArgumentException("Trying to access static MainViewModel instance while it has not been assigned yet");
                }

                return instance;
            }
        }

        private ObservableCollection<NodeWrapper> triggers = null;
        public ObservableCollection<NodeWrapper> Triggers
        {
            get
            {
                if (triggers == null)
                {
                    triggers = new ObservableCollection<NodeWrapper>();
                }

                return triggers;
            }
        }

        private ObservableCollection<NodeWrapper> methods = null;
        public ObservableCollection<NodeWrapper> Methods
        {
            get
            {
                if (methods == null)
                {
                    methods = new ObservableCollection<NodeWrapper>();
                }

                return methods;
            }
        }

        private ObservableCollection<NodeWrapper> variables = null;
        public ObservableCollection<NodeWrapper> Variables
        {
            get
            {
                if (variables == null)
                {
                    variables = new ObservableCollection<NodeWrapper>();
                }

                return variables;
            }
        }
       
        private ObservableCollection<LocalizationItem> localizationStrings = null;
        public ObservableCollection<LocalizationItem> LocalizationStrings
        {
            get
            {
                if (localizationStrings == null)
                {
                    localizationStrings = new ObservableCollection<LocalizationItem>();
                }

                return localizationStrings;
            }
        }

        //public LocalizationData LocalizationStrings { get; set; }

        public ObservableCollection<string> DebugList { get; set; }

        private string statusLabel = null;
        public string StatusLabel
        {
            get
            {
                return statusLabel;
            }
            set
            {
                statusLabel = value;
                RaisePropertyChanged("StatusLabel");
            }
        }

        private string fileLoadInfo = "";
        public string FileLoadInfo
        {
            get
            {
                return fileLoadInfo;
            }
            set
            {
                fileLoadInfo = value;
                RaisePropertyChanged("FileLoadInfo");
            }
        }

        private bool isClassFileName = true;
        public bool IsClassFileName
        {
            get
            {
                return isClassFileName;
            }
            set
            {
                isClassFileName = value;
                RaisePropertyChanged("IsClassFileName");
            }
        }

        private bool isAppend = true;
        public bool IsAppend
        {
            get
            {
                return isAppend;
            }
            set
            {
                isAppend = value;
                RaisePropertyChanged("IsAppend");
            }
        }


        private RelayCommand _OpenCodeWindowCommand;
        public RelayCommand OpenCodeWindowCommand
        {
            get { return _OpenCodeWindowCommand ?? (_OpenCodeWindowCommand = new RelayCommand(openCodeLoadPanelUI)); }
        }

        private RelayCommand _OpenCodeFileFromFileCommand;
        public RelayCommand OpenCodeFileFromFileCommand
        {
            get { return _OpenCodeFileFromFileCommand ?? (_OpenCodeFileFromFileCommand = new RelayCommand(openCodeFromFile)); }
        }

        //Localization
        private RelayCommand _OpenLocalizationWindowCommand;
        public RelayCommand OpenLocalizationWindowCommand
        {
            get { return _OpenLocalizationWindowCommand ?? (_OpenLocalizationWindowCommand = new RelayCommand(openLocalizationLoadPanelUI)); }
        }


        private RelayCommand _OpenLocalizationFile;
        public RelayCommand OpenLocalizationFile
        {
            get { return _OpenLocalizationFile ?? (_OpenLocalizationFile = new RelayCommand(OpenLocalization)); }
        }


        private string _newTriggerName = "Enter trigger name";
        public string NewTriggerName
        {
            get { return _newTriggerName; }
            set
            {
                _newTriggerName = value;
                RaisePropertyChanged("NewTriggerName");
            }
        }

        private RelayCommand _AddTriggerCommand;
        public RelayCommand AddTriggerCommand
        {
            get { return _AddTriggerCommand ?? (_AddTriggerCommand = new RelayCommand(AddNewTrigger)); }
        }

        private RelayCommand<NodeWrapper> _DeleteNodeFromNodeListCommand;
        public RelayCommand<NodeWrapper> DeleteNodeFromNodeListCommand
        {
            get { return _DeleteNodeFromNodeListCommand ?? (_DeleteNodeFromNodeListCommand = new RelayCommand<NodeWrapper>(DeleteNodeFromNodeList)); }
        }


        public void AddNewTrigger()
        {
            if(NewTriggerName != "")
            {
                NodeWrapper newTrigger = new NodeWrapper();
                newTrigger.NodeName = NewTriggerName;
                newTrigger.TypeOfNode = NodeType.RootNode;

                Triggers.Add(newTrigger);

                NewTriggerName = "";
            }
        }

        private void openLocalizationLoadPanelUI()
        {
            OpenLocalizationData w = new OpenLocalizationData();
            w.ShowDialog();
        }

        private void openCodeLoadPanelUI()
        {
            LogStatus("Ready to parse a C# code file", true);
            OpenCodeWindow w = new OpenCodeWindow();
            w.ShowDialog();
        }


        private void openCodeFromFile()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "C# Files (.cs)|*.cs|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            //openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();

            openFileDialog1.Multiselect = true;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog1.ShowDialog();

            int added = 0;
            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                foreach (var file in openFileDialog1.FileNames)
                {
                    OpenCode(file);
                    added++;
                }

                FileLoadInfo = "Added " + added + " code file(s)";
            }
        }

        private void OpenCode(string file)
        {
            //Path.GetFileNameWithoutExtension(file);

            if (!IsAppend)
            {
                Methods.Clear();
                Variables.Clear();
            }

            GetMethods(file, isClassFileName);
            GetVariables(file, isClassFileName);

            string className = Path.GetFileNameWithoutExtension(file);
            LogStatus("C# file " + className + ".cs parsed succesfully", true);
        }

        private void OpenLocalization()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.Filter = "JSON Files (.json)|*.json|All Files (*.*)|*.*";
            openFileDialog1.FilterIndex = 1;
            //openFileDialog1.InitialDirectory = Directory.GetCurrentDirectory();

            openFileDialog1.Multiselect = false;

            // Call the ShowDialog method to show the dialog box.
            bool? userClickedOK = openFileDialog1.ShowDialog();

            int added = 0;
            // Process input if the user clicked OK.
            if (userClickedOK == true)
            {
                string path = openFileDialog1.FileName;
                string json = File.ReadAllText(path);
                try
                {
                    var data = JsonConvert.DeserializeObject<LocalizationData>(json);
                    LocalizationStrings.Clear();

                    foreach (var item in data.Items)
                    {
                        LocalizationStrings.Add(item);
                    }

                    LogStatus("Succesfully parsed " + LocalizationStrings.Count + " localization keys", true);
                }
                catch (Exception e)
                {
                    LogStatus("Json File is not in the correct format", true);
                    throw;
                }
            }
        }

        private RelayCommand _OpenDebug;
        public RelayCommand OpenDebugCommand
        {
            get { return _OpenDebug ?? (_OpenDebug = new RelayCommand(OpenDebug)); }
        }

        public void OpenDebug()
        {
            CodeResultWindow w = new CodeResultWindow();
            w.Show();
        }

        public void GetMethods(string filename, bool isClassNameOnly = false)
        {
            string className = Path.GetFileNameWithoutExtension(filename);
            var syntaxTree = SyntaxTree.ParseFile(filename);
            var root = syntaxTree.GetRoot();

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classNode in classes)
            {
                string cname = classNode.Identifier.ToString();

                //Skip this class entirely if it doesn't match the class name of the code file
                if (isClassNameOnly && cname != className)
                    continue;

                IEnumerable<MethodDeclarationSyntax> methods = classNode.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

                foreach (var method in methods)
                {
                    NodeWrapper node = new NodeWrapper();
                    node.NodeName = method.Identifier.ToString();
                    node.CallingClass = cname;

                    bool isPublic = false;
                    foreach (var mod in method.Modifiers)
                    {
                        if(mod.Kind == SyntaxKind.PublicKeyword)
                        {
                            isPublic = true;
                        }
                    }

                    if (!isPublic)
                        continue;

                    ParameterListSyntax parameters = method.ParameterList;

                    foreach (var param in parameters.Parameters)
                    {
                        Argument a = new Argument();
                        a.Name = param.Identifier.ToString();

                        a.ArgTypeString = param.Type.ToString();
                        node.Arguments.Add(a);
                    }

                    node.TypeOfNode = NodeType.MethodNode;
                    Methods.Add(node);
                }
            }
        }

        public void GetVariables(string filename, bool isClassNameOnly = false)
        {
            string className = Path.GetFileNameWithoutExtension(filename);
            var syntaxTree = SyntaxTree.ParseFile(filename);
            var root = syntaxTree.GetRoot();

            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDef in classes)
            {
                string cname = classDef.Identifier.ToString();

                //Skip this class entirely if it doesn't match the class name of the code file
                if (isClassNameOnly && cname != className)
                    continue;

                IEnumerable<FieldDeclarationSyntax> variables = classDef.DescendantNodes().OfType<FieldDeclarationSyntax>().ToList();

                foreach (var variable in variables)
                {
                    FieldDeclarationSyntax field = variable;
                    VariableDeclarationSyntax var = field.Declaration;
           
                    NodeWrapper node = new NodeWrapper();
                    node.NodeName = var.Variables.First().Identifier.ToString();
                    //node.NodeName = variable.Variables.First().Identifier.Value.ToString();
                    node.CallingClass = cname;

                    
                    bool isPublic = false;
                    foreach (var mod in field.Modifiers)
                    {
                        if(mod.Kind == SyntaxKind.PublicKeyword)
                        {
                            isPublic = true;
                        }
                    }

                    if (!isPublic)
                        continue;
                    
                    node.BaseAssemblyType = var.Type.ToString();
                    node.TypeOfNode = NodeType.VariableNode;
                    Variables.Add(node);
                }
            }
        }

        /*
        public string TypeConverter(string sourceType)
        {
            if (sourceType.ToLower() == "string")
                return "System.String";

            if (sourceType.ToLower() == "bool")
                return "System.Boolean";

            if (sourceType.ToLower() == "float")
                return "System.Single";

            if (sourceType.ToLower() == "int")
                return "System.Int32";

            LogStatus("Couldn't parse type: " + sourceType + ". Using default string type for this object", true);
            return "System.String";
        }
         * */

        
        public void DeleteNodeFromNodeList(NodeWrapper node)
        {
            if (node.TypeOfNode == NodeType.MethodNode)
                Methods.Remove(node);

            if (node.TypeOfNode == NodeType.ConditionNode || node.TypeOfNode == NodeType.RootNode)
                Triggers.Remove(node);

            if (node.TypeOfNode == NodeType.VariableNode)
                Variables.Remove(node);
        }

        public MainViewModel()
        {
            // First we check if there are any other instances conflicting
            if (instance != null && instance != this)
                LogStatus("There's already a Game Manager in the scene, destroying this one.");
            else
                instance = this;

            DebugList = new ObservableCollection<string>();
            LogStatus("MainViewModel initialized.");

            NodeWrapper r = new NodeWrapper();
            r.TypeOfNode = NodeType.RootNode;
            r.NodeName = "GameStart";

            NodeWrapper r2 = new NodeWrapper();
            r2.TypeOfNode = NodeType.RootNode;
            r2.NodeName = "Window Close";

            NodeWrapper con = new NodeWrapper();
            con.TypeOfNode = NodeType.ConditionNode;
            con.NodeName = "Condition";
            con.IsDeletable = false;

            Triggers.Add(r);
            Triggers.Add(r2);
            Triggers.Add(con);
        }

        public void LogStatus(string status, bool showInStatusLabel = false)
        {
            DebugList.Add(status);

            //log
            if(showInStatusLabel)
                StatusLabel = status;

        }
    }
}