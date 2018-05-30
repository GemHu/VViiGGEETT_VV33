/// <summary>
/// @file   AStarPlanner.cs
///	@brief  AStar 路径规划算法的封装接口类。
/// @author	DothanTech 刘伟宏
/// 
/// Copyright(C) 2011~2014, DothanTech. All rights reserved.
/// </summary>

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Dothan.Helpers
{
    /// <summary>
    /// AStar 路径规划算法的封装接口类。
    /// </summary>
    public class AStarPlanner : IDisposable
    {
        /// <summary>
        /// 构建指定宽度和高度矩阵的对象。
        /// </summary>
        /// <param name="width">矩阵宽度，1～255</param>
        /// <param name="height">矩阵高度，1～255</param>
        public AStarPlanner(int width, int height)
        {
            try
            {
                hAStar = AStarInit((byte)width, (byte)height);
            }
            catch (System.Exception ee)
            {
                Trace.WriteLine("###[" + ee.Message + "]; Exception : " + ee.Source);
                Trace.WriteLine("###" + ee.StackTrace);
            }
        }

        /// <summary>
        /// 设置矩阵中的矩形区域被功能块占用了。
        /// </summary>
        /// <param name="gridPos">功能块所占用的单元格矩形区域</param>
        /// <param name="bNearBlock">是否设置矩形区域周边为“靠近功能块”，对于有输入输出管脚的功能块，最好为 true；否则如文本框之类的功能块，要为 false。</param>
        /// <returns>成功与否？</returns>
        public bool OccupiedByBlock(System.Windows.Int32Rect gridPos, bool bNearBlock)
        {
            return this.OccupiedByBlock(gridPos.X, gridPos.Y, gridPos.Width, gridPos.Height, bNearBlock);
        }

        /// <summary>
        /// 设置矩阵中的矩形区域被功能块占用了。
        /// </summary>
        /// <param name="x">矩形区域坐标</param>
        /// <param name="y">矩形区域坐标</param>
        /// <param name="width">矩形区域宽度</param>
        /// <param name="height">矩形区域高度</param>
        /// <param name="bNearBlock">是否设置矩形区域周边为“靠近功能块”，对于有输入输出管脚的功能块，最好为 true；否则如文本框之类的功能块，要为 false。</param>
        /// <returns>成功与否？</returns>
        public bool OccupiedByBlock(int x, int y, int width, int height, bool bNearBlock)
        {
            if (hAStar == IntPtr.Zero)
                return false;

            try
            {
                return AStarSetOccBlock(hAStar, (byte)x, (byte)y, (byte)width, (byte)height, bNearBlock ? 1 : 0) != 0;
            }
            catch (System.Exception ee)
            {
                Trace.WriteLine("###[" + ee.Message + "]; Exception : " + ee.Source);
                Trace.WriteLine("###" + ee.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 设置单元格被需要连线的起始/终止点给占用，必须先设置输出管脚。
        /// </summary>
        /// <param name="gridPos">单元格坐标</param>
        /// <param name="pathIndex">路径标识，从 0 开始，必须和 PathPlan() 的参数相一致。</param>
        /// <returns>成功与否？</returns>
        public bool OccupiedByPath(Int32Point gridPos, int pathIndex)
        {
            return this.OccupiedByPath(gridPos.X, gridPos.Y, pathIndex);
        }

        /// <summary>
        /// 设置单元格被需要连线的起始/终止点给占用，必须先设置输出管脚。
        /// </summary>
        /// <param name="x">单元格坐标</param>
        /// <param name="y">单元格坐标</param>
        /// <param name="pathIndex">路径标识，从 0 开始，必须和 PathPlan() 的参数相一致。</param>
        /// <returns>成功与否？</returns>
        public bool OccupiedByPath(int x, int y, int pathIndex)
        {
            if (hAStar == IntPtr.Zero)
                return false;
            if (pathIndex < 0)
                return false;

            try
            {
                return AStarSetOccupied(hAStar, (byte)x, (byte)y, (ushort)(ASCOT_INOUT + pathIndex)) != 0;
            }
            catch (System.Exception ee)
            {
                Trace.WriteLine("###[" + ee.Message + "]; Exception : " + ee.Source);
                Trace.WriteLine("###" + ee.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// 矩形相同输出管脚引线的路径规划。
        /// </summary>
        /// <param name="pathIndex">路径标识，从 0 开始，必须和 OccupiedByPath() 的参数相一致。</param>
        /// <returns>
        /// null 表示参数有错误；byte[] 个数为 1 表示路径规划失败，路径不通，此时 byte[0] 用于标识不通索引；
        /// 否则是字节数组：路径信息两个字节为一组，分别表示路径拐点的横纵坐标，路径信息中可能包含多
        /// 条路径，用 0x00 0xFF 来分隔路径。0x00 0x00 表示路径信息全部结束。除了第一个路径，后续
        /// 路径都是从交点开始的，该处需要绘制路径交点。
        /// </returns>
        public byte[] PathPlan(int pathIndex)
        {
            if (hAStar == IntPtr.Zero)
                return null;
            if (pathIndex < 0)
                return null;

            try
            {
                int len = AStarPlanPath(hAStar, (ushort)(ASCOT_INOUT + pathIndex));
                if (len < 0 || (len & 1) != 0)
                    return null;

                if (len == 0)
                    return NextUnreachable();

                byte[] buffer = new byte[len];
                if (AStarGetPath(hAStar, out buffer[0], len) == 0)
                    return null;

                return buffer;
            }
            catch (System.Exception ee)
            {
                Trace.WriteLine("###[" + ee.Message + "]; Exception : " + ee.Source);
                Trace.WriteLine("###" + ee.StackTrace);
                return null;
            }
        }

        /// <summary>
        /// 返回不可规划路径的标号路径对象。
        /// </summary>
        /// <returns>长度为 1 的 byte 数组，byte[0] 用于标识不通索引。</returns>
        public byte[] NextUnreachable()
        {
            byte[] unreachable = new byte[1];
            if (UnreachableIndex < 255)
                ++UnreachableIndex;
            unreachable[0] = UnreachableIndex;
            return unreachable;
        }

        public void Dispose()
        {
            // 当前无事可做
        }

        protected IntPtr hAStar = IntPtr.Zero;  ///< AStar 接口句柄
        protected byte UnreachableIndex = 0;    ///< 连线失败路径标号

        protected const ushort ASCOT_INOUT = 6; ///< AStar 标记输入输出管脚位置的类型起始值

        ///*********************************************************************//**
        // * @brief		AStarInit: 初始化指定尺寸的 AStarPlanner。
        // * @param[in]	width: 表格宽度（1~255）。
        // * @param[in]	height: 表格高度（1~255）。
        // * @return 		AStarPlanner 对象标识符，后续函数需要使用该标识符来操作 AStarPlanner。
        // *              返回 NULL 表示初始化失败。
        // * @infor       AStarPlanner 表格坐标为一个字节的数字，从 1 开始，（1~255）。
        // **********************************************************************/
        //HANDLE WINAPI AStarInit(BYTE width, BYTE height);
        [DllImport("AStarPlanner.dll")]
        protected static extern IntPtr AStarInit(byte width, byte height);

        ///*********************************************************************//**
        // * @brief		AStarSetOccBlock: 设置 AStarPlanner 单元格的被功能块占用。
        // * @param[in]	hAStar: AStarInit() 返回的 AStarPlanner 对象标识符。
        // * @param[in]	x: 单元格位置（1~255）。
        // * @param[in]	y: 单元格位置（1~255）。
        // * @param[in]	width: 功能块宽度（1~255）。
        // * @param[in]	height: 功能块高度（1~255）。
        // * @param[in]	bNearBlock: 是否将功能块的周边，设置成 ASCOT_NEARB，以尽量空出空间来走线？
        // * @return 		void
        // **********************************************************************/
        //BOOL WINAPI AStarSetOccBlock(HANDLE hAStar, BYTE x, BYTE y, BYTE width, BYTE height, BOOL bNearBlock);
        [DllImport("AStarPlanner.dll")]
        protected static extern int AStarSetOccBlock(IntPtr hAStar, byte x, byte y, byte width, byte height, int bNearBlock);

        ///*********************************************************************//**
        // * @brief		AStarSetOccupied: 设置 AStarPlanner 单元格的被占用情况。
        // * @param[in]	hAStar: AStarInit() 返回的 AStarPlanner 对象标识符。
        // * @param[in]	x: 单元格位置（1~255）。
        // * @param[in]	y: 单元格位置（1~255）。
        // * @param[in]	occupiedType: 单元格占用情况，ASCOT_xxx。
        // * @return 		void
        // **********************************************************************/
        //BOOL WINAPI AStarSetOccupied(HANDLE hAStar, BYTE x, BYTE y, WORD occupiedType);
        [DllImport("AStarPlanner.dll")]
        protected static extern int AStarSetOccupied(IntPtr hAStar, byte x, byte y, ushort occupiedType);

        ///*********************************************************************//**
        // * @brief		AStarPlan: 进行 AStarPlanner 路径规划。
        // * @param[in]	hAStar: AStarInit() 返回的 AStarPlanner 对象标识符。
        // * @param[in]	occupiedType: 单元格占用情况，ASCOT_INPUT + n。
        // * @return 		返回大于 0 表示规划后路径信息需要占用的字节数，可以根据此分配内存，
        // *              然后再调用 AStarGetPath() 得到规划的路径信息。
        // *              返回 0 表示路径规划失败，返回小于 0 表示参数错误。
        // **********************************************************************/
        //int WINAPI AStarPlanPath(HANDLE hAStar, WORD occupiedType);
        [DllImport("AStarPlanner.dll")]
        protected static extern int AStarPlanPath(IntPtr hAStar, ushort occupiedType);

        ///*********************************************************************//**
        // * @brief		AStarGetPath: 得到上一次 AStarPlanner 路径规划的路径信息。
        // * @param[in]	hAStar: AStarInit() 返回的 AStarPlanner 对象标识符。
        // * @param[out]	pathInfo: 用于返回路径信息。路径信息两个字节为一组，分别表示路径拐
        // *              点的横纵坐标，路径信息中可能包含多条路径，用 0x00 0xFF 来分隔路径。
        // *              0x00 0x00 表示路径信息全部结束。除了第一个路径，后续路径都是从交点
        // *              开始的，该处需要绘制路径交点。
        // * @param[in]	pathSize: 路径信息的内存大小（字节数）。
        // * @return 		返回成功还是失败？
        // **********************************************************************/
        //BOOL WINAPI AStarGetPath(HANDLE hAStar, BYTE * pathInfo, int pathSize);
        [DllImport("AStarPlanner.dll")]
        protected static extern int AStarGetPath(IntPtr hAStar, out byte pathInfo, int PathSize);
    }
}
