using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace UnityFlow
{
    /**********************************************************************************************************
     *             Logic to handle the reading of a XML node flowchart made with CoffeeFlow, 
     *             allows node traversal, firing triggers and calling of functions on the class that the nodes belong to after having
     *             added the class with AddCaller([instance of class])
     *                                                      * Nick @ http://immersivenick.wordpress.com 
     *                                                      * Free for non-commercial use
     * *********************************************************************************************************/
    public enum NodeType { RootNode, MethodNode, VariableNode, LogicNode, ConditionNode };

    public class FlowParser
    {
        List<SerializeableNodeViewModel> SerializeNodes;
        public bool IsLoadedFromFile = false;
        public bool IsActive = false; //we are currently on no node

        SerializeableNodeViewModel current;
        List<object> callingClasses;
        public List<string> ErrorOutput { get; private set; }

        public void AddCaller(object obj)
        {
            if (obj != null)
                this.callingClasses.Add(obj);
        }

        public FlowParser()
        {
            SerializeNodes = new List<SerializeableNodeViewModel>();
            callingClasses = new List<object>();

            ErrorOutput = new List<string>();
        }

        public FlowParser(string pathToNodes)
        {
            SerializeNodes = new List<SerializeableNodeViewModel>();
            LoadNodes(pathToNodes);

            ErrorOutput = new List<string>();
        }

        public void LoadNodes(string path)
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<SerializeableNodeViewModel>), new Type[] { typeof(SerializeableVariableNode), typeof(SerializeableDynamicNode), typeof(SerializeableConditionNode), typeof(SerializeableRootNode) });

            using (XmlReader reader = XmlReader.Create(path))
            {
                SerializeNodes = (List<SerializeableNodeViewModel>)ser.Deserialize(reader);
            }

            IsLoadedFromFile = true;
        }

        private SerializeableNodeViewModel getTrigger(string triggerName)
        {
            IEnumerable<SerializeableNodeViewModel> results = SerializeNodes.Where(n => n.NodeName == triggerName);

            if (results.Count() == 1)
            {
                return results.First();
            }

            return null;
        }

        private SerializeableNodeViewModel getNodeByID(int id)
        {
            IEnumerable<SerializeableNodeViewModel> results = SerializeNodes.Where(n => n.ID == id);

            if (results.Count() == 1)
                return results.First();

            return null;
        }

        public bool FireTrigger(string name)
        {
            current = getTrigger(name);

            if (current != null)
            {
                IsActive = true;
                return true;
            }

            return false;
        }

        public NodeWrapper GetCurrentAction()
        {
            NodeWrapper node = new NodeWrapper();
            node.NodeName = "[UNDEFINED]";
            node.Arguments = new List<Argument>();
            node.CallingClass = "";

            if (current != null && current.NodeType == NodeType.MethodNode)
            {
                SerializeableDynamicNode method = current as SerializeableDynamicNode;
                node.NodeName = method.NodeName;
                node.Arguments = method.Arguments;
                node.CallingClass = method.CallingClass;
            }

            return node;
        }

        public SerializeableNodeViewModel GoToNextNode()
        {
            if (current != null)
            {
                int nextID = 0;
                if (current.NodeType == NodeType.RootNode)
                {
                    SerializeableRootNode root = current as SerializeableRootNode;
                    nextID = root.OutputNodeID;
                }

                if (current.NodeType == NodeType.MethodNode)
                {
                    SerializeableDynamicNode method = current as SerializeableDynamicNode;
                    nextID = method.OutputNodeID;
                }

                if(current.NodeType == NodeType.ConditionNode)
                {
                    SerializeableConditionNode condition = current as SerializeableConditionNode;
                    string varToCheck = condition.BoolVariableName;
                    bool toCheck = getBoolVariableValue(condition.BoolVariableName, condition.BoolCallingClass);
                    
                    if(toCheck == true)
                    {
                        nextID = condition.OutputTrueNodeID;
                    }
                    else
                    {
                        nextID = condition.OutputFalseNodeID;
                    }
                }

                current = getNodeByID(nextID);
            }

            //mark as not active
            if (current == null)
                IsActive = false;

            return current;
        }

        public NodeType CurrentNodeType()
        {
            return current.NodeType;
        }

        public void ExecuteCurrentAction()
        {
            //TODO: Save method caller in grid serialization
            NodeWrapper method = GetCurrentAction();
            CallFunction(method.NodeName, method.Arguments, method.CallingClass);

        }

        private bool getBoolVariableValue(string varName, string callerName)
        {
            object caller = null;

            //todo get name from within method
            foreach (var item in callingClasses)
            {
                string fullname = item.GetType().Name;
                if (fullname == callerName)
                    caller = item;
            }

            object result = caller.GetType().GetField(varName).GetValue(caller);

            if(result != null)
                return (bool)result;

            ErrorOutput.Add("Couldn't get bool value for variable " + varName + " in " + callerName);
            return false;
        }

        public void CallFunction(string methodName, List<Argument> arguments, string callerName)
        {
            object caller = null;

            //todo get name from within method
            foreach (var item in callingClasses)
            {
                string fullname = item.GetType().Name;
                if (fullname == callerName)
                    caller = item;
            }

            if (caller == null)
                return;

            Type thisType = caller.GetType();
            MethodInfo theMethod = thisType.GetMethod(methodName);

            List<object> argumentList = new List<object>();
            foreach (var argument in arguments)
            {
                if (argument.ArgIsExistingVariable) //parameter from runtime field
                {
                    object result = caller.GetType().GetField(argument.ArgExistingVariableName).GetValue(caller);
                    argumentList.Add(result);
                }
                else //new parameter from XML
                {
                    Type t = Type.GetType(TypeConverter(argument.ArgTypeString));

                    if (t == typeof(string))
                    {
                        argumentList.Add((string)argument.ArgValue);
                    }

                    if (t == typeof(float))
                    {
                        float val = Convert.ToSingle(argument.ArgValue);
                        argumentList.Add(val);
                    }

                    if (t == typeof(bool))
                    {
                        argumentList.Add((bool)argument.ArgValue);
                    }

                    if (t == typeof(int))
                    {
                        argumentList.Add((int)argument.ArgValue);
                    }

                    //more types
                }
            }

            theMethod.Invoke(caller, argumentList.ToArray());
        }

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

            //TODO log error.
            return "System.String";
        }
    }

    [Serializable]
    public abstract class SerializeableNodeViewModel
    {
        private NodeType _nodeType;
        public NodeType NodeType
        {
            get { return _nodeType; }
            set
            {
                _nodeType = value;

            }
        }

        public virtual string GetSerializationString()
        {
            return "";
        }

        public string NodeDataString { get; set; }
        public string NodeDescription { get; set; }
        public bool CanDrag = true;

        public static event EventHandler<EventArgs> Closed;

        public int ID;
        public static int TotalIDCount = 0;

        public string NodeName;
        public string CallingClass;

        public double MarginX;
        public double MarginY;
        public string Debug { get; set; }

        public bool IsDraggable = true;
    }

    [Serializable]
    public class SerializeableDynamicNode : SerializeableNodeViewModel
    {
        public string Command;

        public List<Argument> Arguments { get; set; }

        public int InputNodeID;
        public int OutputNodeID;

        public int NodePanelHeight;

        public SerializeableDynamicNode()
        {
            Arguments = new List<Argument>();
        }
    }

    [Serializable]
    public class SerializeableConditionNode : SerializeableNodeViewModel
    {
        public int InputNodeID;
        public int OutputFalseNodeID;
        public int OutputTrueNodeID;
        public int BoolVariableID;
        public string BoolVariableName;
        public string BoolCallingClass;

        public SerializeableConditionNode()
        {
           
        }
    }

    [Serializable]
    public class SerializeableRootNode : SerializeableNodeViewModel
    {
        public int OutputNodeID;
    }

    public enum VariableKind
    {
        Field, LocalVariable, MagicNumber, NotifyChangedMember
    }

    [Serializable]
    public class SerializeableVariableNode : SerializeableNodeViewModel
    {
        public string TypeString;

        public VariableKind KindOfVariable;
        public int ConnectedToNodeID;
        public int ConnectedToConnectorID;
    }

    [Serializable]
    public class Argument
    {
        public string Name { get; set; }
        public string ArgTypeString { get; set; }
        public object ArgValue { get; set; }

        public bool ArgIsExistingVariable { get; set; }
        public string ArgExistingVariableName { get; set; }
        public int ArgumentConnectedToNodeID { get; set; }
        public int ArgumentConnectorID { get; set; }

        public Argument(string name, string typeString)
        {
            this.Name = name;
            this.ArgTypeString = typeString;
            this.ArgExistingVariableName = "";
        }

        public Argument()
        {
            this.ArgExistingVariableName = "";
        }
    }

}
