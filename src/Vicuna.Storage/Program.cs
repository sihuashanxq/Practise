using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Text;
using Vicuna.Storage.Tree;
using Vicuna.Storage.Trees;

namespace Vicuna.Storage
{
    public class Program
    {
        public static List<TreeNodePage> Pages = new List<TreeNodePage>();

        public static void Main()
        {
            var node = new TreeNodePage(new StoragePageManager(null))
            {

            };

            for (var i = 0; i < 10000; i++)
            {
                node.InsertKey(new ByteString(Encoding.UTF8.GetBytes(i.ToString().PadRight(6, '0'))), new StoragePosition(), out _, out _);
            }
        }
    }
}
