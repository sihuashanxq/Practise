namespace Vicuna.Storage.Trees
{
    public partial class TreeNodePage
    {
        /// <summary>
        /// </summary>
        /// <param name="comparedKey"></param>
        /// <returns></returns>
        public int SearchKey(ByteString searchKey)
        {
            for (var i = 0; i < Keys.Count; i++)
            {
                var key = Keys[i];
                if (key.Compare(searchKey) > 0)
                {
                    return i;
                }
            }

            return IsLeaf ? -1 : Keys.Count;
        }

        public int SearchKey(ByteString searchKey, out TreeNodePage treePage)
        {
            // 在当前节点中找到键应该在的位置
            var index = SearchKey(searchKey);
            if (index == -1)
            {
                treePage = null;
                return index;
            }

            //查找结束
            if (IsLeaf)
            {
                treePage = this;
                return index;
            }

            treePage = null;
            //Load Child 
            return -1;
        }
    }
}
