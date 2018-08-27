using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityFlow
{
    /**********************************************************************************************************
 *             A wrapper for nodes without the exessive data, used in the flow parser to represent nodes 
 *             as well as on the UI side of the tool to display the nodes in lists without fully instantiating an entire UI object
 *                                                      * Nick @ http://immersivenick.wordpress.com 
 *                                                      * Free for non-commercial use
 * *********************************************************************************************************/
    public class NodeWrapper
    {
        public string NodeName { get; set; }
        public string NodeDescription { get; set; }
        public List<Argument> Arguments;
        public NodeType TypeOfNode;
        public string BaseAssemblyType { get; set; }
        public string CallingClass { get; set; }
        public bool IsDeletable { get; set; }

        public string DetailString { get { return GetDetailStringBasedOnType(); } }

        public string GetDetailStringBasedOnType()
        {
            string arg = "";
            if(TypeOfNode == NodeType.MethodNode)
            {
                return " in " + CallingClass;
            }

            if (TypeOfNode == NodeType.VariableNode)
            {
                    arg += BaseAssemblyType;
                    arg += " in " + CallingClass;
            }

            if (TypeOfNode == NodeType.ConditionNode)
            {
                return "True/false flow boolean condition";
            }

            if (TypeOfNode == NodeType.RootNode)
            {
                return "Trigger";
            }

            return arg;
        }
        public NodeWrapper()
        {
            Arguments = new List<Argument>();
            IsDeletable = true;
        }

        public NodeWrapper(string name)
        {
            this.NodeName = name;
            Arguments = new List<Argument>();
            IsDeletable = true;
        }
    }
}
