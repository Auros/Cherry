using UnityEngine;
using Cherry.Models;
using static BeatSaberMarkupLanguage.Components.CustomListTableData;

namespace Cherry.UI
{
    public class RequestCellInfo : CustomCellInfo
    {
        internal readonly Map map;
        public readonly RequestEventArgs request;

        internal RequestCellInfo(RequestEventArgs args, Map map, Sprite icon) : base(map.Name, map.Uploader.Name, icon)
        {
            request = args;
            this.map = map;
        }
    }
}
