using Dothan.ViObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dothan.Helpers;
using System.IO;

namespace DothanTech.ViGET.Manager
{
    public abstract class ViTaskInfo : ViNamedObject<ViCPUInfo>
    {
        public ViTaskInfo(String name, int priority)
        {
        }

        public new  String Type
        {
            get { return base.Type as String; }
            set { base.Type = value; }
        }

        public int Priority { get; set; }

        #region TaskFiles

        public ViObservableCollection<TaskFile> TaskFiles
        {
            get
            {
                if (this._taskFiles == null)
                    this._taskFiles = new ViObservableCollection<TaskFile>();

                return this._taskFiles;
            }
        }
        private ViObservableCollection<TaskFile> _taskFiles;

        /// <summary>
        /// 更新Task中文件的执行顺序；
        /// </summary>
        public void UpdatePriority()
        {
            int index = 0;
            foreach (var item in this.TaskFiles)
            {
                item.ExectionOrder = ++index;
            }
        }

        public void AddTaskFile(TaskFile file)
        {
            if (file == null)
                return;

            file.SetParent(this);
            this.TaskFiles.SortedAdd(file);
        }

        /// <summary>
        /// 通过递归查找方式获取给定名称的文件信息；
        /// </summary>
        public ViFileInfo GetFileInfo(String name)
        {
            return this.Parent == null ? null : this.Parent.GetFileInfo(name);
        }

        #endregion

        public override int CompareTo(ViNamedObject other)
        {
            if (this.GetType().Equals(other.GetType()))
                return this.Priority - (other as ViTaskInfo).Priority;
            if (this is CyclicTaskInfo)
                return -1;

            return 1;
        }

        /// <summary>
        /// 根据给定的类型，创建目标任务实例；
        /// </summary>
        public static ViTaskInfo CreateTaskInfo(String type, String name, int priority)
        {
            if (CyclicTaskInfo.TASK_TYPE.Equals(type, StringComparison.OrdinalIgnoreCase))
                return new CyclicTaskInfo(name, priority);
            else if (InterruptTaskInfo.TASK_TYPE.Equals(type, StringComparison.OrdinalIgnoreCase))
                return new InterruptTaskInfo(name, priority);

            return null;
        }

        /// <summary>
        /// 从XMLElement中读取相关有效数据；
        /// </summary>
        public bool LoadElement(XmlElement element)
        {
            if (element == null || !Constants.TAG.Task.Equals(element.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            foreach (XmlNode fileElement in element.ChildNodes)
            {
                if (Constants.TAG.File.Equals(fileElement.Name, StringComparison.OrdinalIgnoreCase))
                {
                    String fileName = (fileElement as XmlElement).GetAttribute(Constants.Attribute.FileName);
                    String order = (fileElement as XmlElement).GetAttribute(Constants.Attribute.ExectionOrder);
                    int exectionOrder = Int32.Parse(order);
                    this.TaskFiles.SortedAdd(new TaskFile(Path.GetFileNameWithoutExtension(fileName), exectionOrder));
                }
            }

            return true;
        }

        /// <summary>
        /// 将相关的配置信息保存到XMLElement中；
        /// </summary>
        public bool SaveElement(XmlElement parentElement, XmlDocument doc)
        {
            XmlElement element = doc.CreateElement(Constants.TAG.Task);
            parentElement.AppendChild(element);
            element.SetAttribute(Constants.Attribute.Name, this.Name);
            element.SetAttribute(Constants.Attribute.Type, this.Type);
            element.SetAttribute(Constants.Attribute.Priority, this.Priority.ToString());

            foreach (var item in this.TaskFiles)
            {
                XmlElement fileElement = doc.CreateElement(Constants.TAG.File);
                element.AppendChild(fileElement);
                fileElement.SetAttribute(Constants.Attribute.ExectionOrder, item.ExectionOrder.ToString());
                fileElement.SetAttribute(Constants.Attribute.FileName, item.Name);
            }

            return true;
        }
    }

    public class CyclicTaskInfo : ViTaskInfo
    {
        public const String TASK_TYPE = "cyclic";

        public CyclicTaskInfo(String name, int priority)
            : base(name, priority)
        {
            this.Type = TASK_TYPE;
        }
    }

    public class InterruptTaskInfo : ViTaskInfo
    {
        public const String TASK_TYPE = "interrupt";

        public InterruptTaskInfo(String name, int priority)
            : base(name, priority)
        {
            this.Type = TASK_TYPE;
        }
    }

    public class TaskFile : ViNamedObject<ViTaskInfo>
    {
        public TaskFile(String name, int order)
            : base(name)
        {
            this.ExectionOrder = order;
        }

        #region 基本属性

        /// <summary>
        /// 当前文件的执行顺序；
        /// </summary>
        public int ExectionOrder { get; set; }

        public ViFileInfo TargetFile
        {
            get { return this.Parent == null ? null : this.Parent.GetFileInfo(this.Name); }
        }

        #endregion

        public override int CompareTo(ViNamedObject other)
        {
            if (other is TaskFile)
                return this.ExectionOrder - (other as TaskFile).ExectionOrder;

            return base.CompareTo(other);
        }
    }
}
