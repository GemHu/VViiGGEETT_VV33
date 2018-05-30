/// <summary>
/// @file   ViPouSource.cs
///	@brief  ViGET 功能块数据的来源。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

using Dothan.Helpers;

namespace Dothan.ViObject
{
    /// <summary>
    /// POU（功能块）特性。
    /// </summary>
    [Flags]
    public enum ViPouAttributes
    {
        None = 0x0000,                      ///< 无效特性

        LDWrapper = 0x0001,                 ///< 从 LDWrapper 中获取的功能块
        Manufacturer = 0x0002,              ///< 制造商提供的功能块
        CFCFBDStandardFuns = 0x0004,        ///< CFCFBD 的标准函数功能块

        Library = 0x0010,                   ///< 属于库类型的功能块
        ProjectLibrary = 0x0020 | Library,  ///< 工程引用的功能块库
        CPULibrary = 0x0040 | Library,      ///< CPU 引用的功能块库
        ProjectUser = 0x0080,               ///< 工程自定义的功能块库

        Public = 0x1000,                    ///< 公共类型，都可以
        Protected = 0x2000,                 ///< 保护类型，受限可见

        Function = 0x4000,                  ///< FUNCTION
        FunctionBlock = 0x8000,             ///< FUNCTION_BLOCK
    };

    /// <summary>
    /// ViGET 功能块数据的来源。
    /// </summary>
    public abstract class ViPouSource : MappedTreeObject
    {
        #region Life cycle

        /// <summary>
        /// 构建对象。
        /// </summary>
        /// <param name="sourceFile">功能块数据来源对应的文件。</param>
        /// <param name="pouAttributes">数据来源中的功能块的特性。</param>
        public ViPouSource(string sourceFile, ViPouAttributes pouAttributes)
        {
            this.SourceFile = sourceFile;
            this.PouAttributes = pouAttributes;

            // 设置数据为未准备好状态
            this.SetDirty();
        }

        /// <summary>
        /// 搜索时是否递归进入子对象进行搜索？
        /// </summary>
        protected virtual bool RecursiveSearch { get { return false; } }

        #endregion

        /// <summary>
        /// 功能块数据来源对应的文件。
        /// </summary>
        public string SourceFile { get; protected set; }

        /// <summary>
        /// 数据来源中的功能块的特性。
        /// </summary>
        public ViPouAttributes PouAttributes { get; protected set; }

        /// <summary>
        /// 重载转化成字符串的函数，调试时可以很方便地显示对象信息。
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}({1}): {2}", this.SourceFile, this.PouAttributes.ToString(), this.Children.Count);
        }

        #region Children

        /// <summary>
        /// 功能块数组。
        /// </summary>
        public new ViObservableCollection<ViNamedObject> Children
        {
            get
            {
                // 功能块数组尚未准备好时，准备数据
                if (this.childrenIsDirty)
                {
                    this.childrenIsDirty = false;

                    // 更新 POU 列表
                    UpdatePOUs();
                }

                return base.Children;
            }
        }

        #region SortedPOUs

        /// <summary>
        /// 为了方便界面绑定排序之后的 POUs 集合，增加根据 POU 名称进行排序的 SortedPOUs 属性。
        /// @warning 该集合不监控子集合的变化事件，因此只建议临时/模式对话框中使用，不建议长时间保存。
        /// </summary>
        public ViObservableCollection<ViFirmBlockType> SortedPOUs
        {
            get
            {
                ViObservableCollection<ViFirmBlockType> pous =
                    new ViObservableCollection<ViFirmBlockType>();
                this.LoopPOUs((pou) =>
                {
                    pous.SortedAdd(pou);
                    return true;
                });
                return pous;
            }
        }

        #endregion

        /// <summary>
        /// 更新功能块数组列表。
        /// </summary>
        public abstract void UpdatePOUs();

        /// <summary>
        /// 设置功能块数据未准备好状态。
        /// </summary>
        public virtual void SetDirty()
        {
            this.childrenIsDirty = true;
        }
        /// <summary>
        /// 子对象数组是否是失效的？
        /// </summary>
        protected bool childrenIsDirty = true;

        #endregion

        #region POU by Name

        /// <summary>
        /// 查找指定名称的功能块。
        /// </summary>
        /// <param name="name">功能块名称（不区分大小写）</param>
        /// <returns>指定名称的功能块。</returns>
        public virtual ViFirmBlockType GetPOU(string name)
        {
            // 先保证数据被更新了
            object _Children = this.Children;

            ViFirmBlockType blockType;

            // 先在子节点中查找
            name = name.ToUpper();
            if (this.Children != null && this.Children.Count > 0)
            {
                blockType = this.Children.Where(o => string.Compare(o.Name, name, true) == 0).FirstOrDefault() as ViFirmBlockType ;
               
                if (blockType != null)
                    return blockType;
            }

            if (this.RecursiveSearch)
            {
                // 在子节点中递归查找
                foreach (ViNamedObject child in this.Children)
                {
                    ViPouSource pouSource = child as ViPouSource;
                    if (pouSource == null) continue;

                    blockType = pouSource.GetPOU(name);
                    if (blockType != null)
                        return blockType;
                }
            }

            // 没有找到
            return null;
        }

        #endregion

        #region LoopPOUs

        /// <summary>
        /// 遍历所有的功能块，如果 loopFunc 返回 false，则停止遍历。
        /// </summary>
        /// <param name="loopFunc">功能块处理函数，如果返回 false，则停止遍历</param>
        /// <returns>遍历是否没有被 loopFunc 函数终止？</returns>
        public virtual bool LoopPOUs(Func<ViFirmBlockType, bool> loopFunc)
        {
            return this.LoopPOUs(loopFunc, null);
        }

        /// <summary>
        /// 遍历所有的功能块，如果 loopFunc 返回 false，则停止遍历。
        /// </summary>
        /// <param name="loopFunc">功能块处理函数，如果返回 false，则停止遍历</param>
        /// <param name="pouSourceFilter">对嵌套的 Pou Source 对象进行过滤，如果返回 false，则该 Pou Source 对象就不遍历了。</param>
        /// <returns>遍历是否没有被 loopFunc 函数终止？</returns>
        public virtual bool LoopPOUs(Func<ViFirmBlockType, bool> loopFunc, Func<ViPouSource, bool> pouSourceFilter)
        {
            return this.LoopPOUs(loopFunc, pouSourceFilter, new Dictionary<string, ViFirmBlockType>());
        }

        /// <summary>
        /// 遍历所有的功能块，如果 loopFunc 返回 false，则停止遍历。
        /// </summary>
        /// <param name="loopFunc">功能块处理函数，如果返回 false，则停止遍历</param>
        /// <param name="pouSourceFilter">对嵌套的 Pou Source 对象进行过滤，如果返回 false，则该 Pou Source 对象就不遍历了。</param>
        /// <param name="child">子对象</param>
        /// <param name="loopedTypes">用于记录已经被遍历过的功能块信息</param>
        /// <returns>遍历是否没有被 loopFunc 函数终止？</returns>
        protected virtual bool LoopPOUs(Func<ViFirmBlockType, bool> loopFunc, Func<ViPouSource, bool> pouSourceFilter, Dictionary<string, ViFirmBlockType> loopedTypes)
        {
            foreach (ViNamedObject child in this.Children)
            {
                if (!this.LoopPOUs(loopFunc, pouSourceFilter, child, loopedTypes))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 遍历所有的功能块，如果 loopFunc 返回 false，则停止遍历。
        /// </summary>
        /// <param name="loopFunc">功能块处理函数，如果返回 false，则停止遍历</param>
        /// <param name="pouSourceFilter">对嵌套的 Pou Source 对象进行过滤，如果返回 false，则该 Pou Source 对象就不遍历了。</param>
        /// <param name="child">子对象</param>
        /// <param name="loopedTypes">用于记录已经被遍历过的功能块信息</param>
        /// <returns>遍历是否没有被 loopFunc 函数终止？</returns>
        protected virtual bool LoopPOUs(Func<ViFirmBlockType, bool> loopFunc, Func<ViPouSource, bool> pouSourceFilter, ViNamedObject child, Dictionary<string, ViFirmBlockType> loopedTypes)
        {
            if (child is ViFirmBlockType)
            {
                ViFirmBlockType blockType = child as ViFirmBlockType;

                // 记录遍历过的功能块信息
                string key = blockType.Name.ToUpper();
                if (loopedTypes.ContainsKey(key))
                    return true;
                loopedTypes[key] = blockType;

                // 遍历功能块
                if (!loopFunc(blockType))
                    return false;
            }
            else if (child is ViPouSource)
            {
                ViPouSource pouSource = child as ViPouSource;
                if (pouSourceFilter == null || pouSourceFilter(pouSource))
                {
                    if (!pouSource.LoopPOUs(loopFunc, pouSourceFilter, loopedTypes))
                        return false;
                }
            }

            return true;
        }

        #endregion
    }
}
