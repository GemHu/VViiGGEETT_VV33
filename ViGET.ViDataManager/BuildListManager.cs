/// <summary>
/// @file   BuildListManager.cs
///	@brief  ViGET 工程的 CPU 编译信息列表管理器，一个工程有一个这样的对象，管理工程下的 CPU 编译信息列表。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows;
using System.ComponentModel;

namespace Dothan.Manager
{
    /// <summary>
    /// ViGET 工程的 CPU 编译信息列表管理器，一个工程有一个这样的对象，管理工程下的 CPU 编译信息列表。
    /// </summary>
    public class BuildListManager : IBuildListManager, IViGETManager
    {
        /// <summary>
        /// Object info is changed or not.
        /// </summary>
        public bool Dirty { get; set; }

        private List<BuildPlan> listOfPlansToBeBuild = new List<BuildPlan>();

        private string projectPath = string.Empty;

        public BuildListManager(string projectPath)
        {
            this.projectPath = projectPath;

            // reset dirty flag
            Dirty = false;
        }

        public void Init(XmlNodeList planList)
        {
            listOfPlansToBeBuild.Clear();

            try
            {
                foreach (XmlNode node in planList)
                {
                    string resource = node.SelectSingleNode("Resource").InnerText;
                    string planName = node.SelectSingleNode("PlanName").InnerText;
                    listOfPlansToBeBuild.Add(new BuildPlan(planName, resource));
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                // reset dirty flag
                Dirty = false;
            }
        }

        public List<BuildPlan> GetPlanList()
        {
            return listOfPlansToBeBuild;
        }

        public void Register(string planName, string resource)
        {
            if (listOfPlansToBeBuild.FindIndex(p => p.PlanName == planName.ToUpper() && p.Resource == resource.ToUpper()) < 0)
            {
                listOfPlansToBeBuild.Add(new BuildPlan(planName.ToUpper(), resource.ToUpper()));

                // set dirty flag
                Dirty = true;
            }
        }

        public void Unregister(string planName)
        {
            listOfPlansToBeBuild.RemoveAll(p => p.PlanName == planName.ToUpper());

            // set dirty flag
            Dirty = true;
        }

        public void RenamePlan(string oldName, string newName)
        {
            foreach (var plan in listOfPlansToBeBuild)
            {
                if (plan.PlanName.ToUpper() == oldName.ToUpper())
                {
                    plan.PlanName = newName.ToUpper();

                    // set dirty flag
                    Dirty = true;
                }
            }
        }

        public void RenameResource(string oldName, string newName)
        {
            foreach (var plan in listOfPlansToBeBuild)
            {
                if (plan.Resource.ToUpper() == oldName.ToUpper())
                {
                    plan.Resource = newName.ToUpper();

                    // set dirty flag
                    Dirty = true;
                }
            }
        }

        public void ChangeResource(string resource, string resourceType, string planName)
        {
            var itemsToRename = listOfPlansToBeBuild.Where(b => b.PlanName.ToUpper().Equals(planName.ToUpper())).ToList();
            foreach (var itemToRename in itemsToRename)
            {
                itemToRename.Resource = resource.ToUpper();

                // set dirty flag
                Dirty = true;
            }
        }

        //return the list of CFC plans belonging to the given resource, for which a new POE must be generated
        public string GetListOfNewPOEs(string resourceName)
        {
            string ret = string.Empty;
            List<BuildPlan> listOfPlan = listOfPlansToBeBuild.Where(p => p.Resource.ToUpper() == resourceName.ToUpper()).ToList();
            foreach (var item in listOfPlan)
            {
                ret += item.PlanName + ";";                
            }

            listOfPlansToBeBuild.RemoveAll(p => p.Resource.ToUpper() == resourceName.ToUpper());
            return ret;
        }
    }

    /// <summary>
    /// CPU 编译信息列表管理器管理的编译条目信息。
    /// </summary>
    public class BuildPlan
    {
        private string planName;

        public string PlanName
        {
            get { return planName; }
            set { planName = value; }
        }
        private string resource;

        public string Resource
        {
            get { return resource; }
            set { resource = value; }
        }

        public BuildPlan(string planName, string resource)
        {
            this.planName = planName;
            this.resource = resource;
        }

        public XmlNode GetXmlBlockNode(XmlDocument doc)
        {
            XmlElement Plan = doc.CreateElement("Plan");

            XmlElement resource = doc.CreateElement("Resource");
            resource.InnerText = this.resource;
            Plan.AppendChild(resource);

            XmlElement PlanName = doc.CreateElement("PlanName");
            PlanName.InnerText = this.planName;
            Plan.AppendChild(PlanName);

            return Plan;
        }
    }
}
