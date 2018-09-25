using System;
using System.Collections.Generic;
using System.Text;

namespace Vicuna.Storage.Trees
{
    public class Constants
    {
        /// <summary>
        /// 8字节用于存放页面引用
        /// </summary>
        public const int PageReferenceBytes = 8;

        /// <summary>
        /// 1字节用于存放节点类型
        /// </summary>
        public const int NodeTypeBytes = 1;

        /// <summary>
        /// 3字节用于存放页面有效字节数
        /// </summary>
        public const int UsedPageLengthBytes = 3;

        /// <summary>
        /// 2字节存放使用的key长度字节
        /// </summary>
        public const int UsedKeyLengthBytes = 2;

        /// <summary>
        /// 2字节存放Value长度字节
        /// </summary>
        public const int UsedValueLengthBytes = 2;

        /// <summary>
        /// 每一帧最多256种类型
        /// </summary>
        public const int FrameTypeBytes = 1;

        /// <summary>
        /// 其他4字节
        /// </summary>
        public const int OthersBytes = 4;
    }
}
