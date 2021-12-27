
 
private void Outline(ref CuiElementContainer container, string parent, string color = "1 1 1 1", string size = "2.5")
{
container.Add(new CuiPanel
{
    RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0", OffsetMin = $"0 0", OffsetMax = $"0 {size}" },
    Image = { Color = color }
}, parent);
container.Add(new CuiPanel
{
    RectTransform = { AnchorMin = "0 1", AnchorMax = "1 1", OffsetMin = $"0 -{size}", OffsetMax = $"0 0" },
    Image = { Color = color }
}, parent); 
container.Add(new CuiPanel 
{
    RectTransform =
    {AnchorMin = "0 0", AnchorMax = "0 1", OffsetMin = $"0 {size}", OffsetMax = $"{size} -{size}"},
    Image = { Color = color }
}, parent);
container.Add(new CuiPanel
{
    RectTransform =
    {AnchorMin = "1 0", AnchorMax = "1 1", OffsetMin = $"-{size} {size}", OffsetMax = $"0 -{size}"},
    Image = { Color = color }
}, parent);
}


