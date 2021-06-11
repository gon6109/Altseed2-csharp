using System.Collections.Generic;

namespace Altseed2
{
    public interface IToolElementTreeFactory
    {
        GuiInfoRepository GuiInfoRepository { get; }
        IEnumerable<ToolElement> CreateToolElements(object source);
    }
}
