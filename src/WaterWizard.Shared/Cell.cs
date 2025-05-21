namespace WaterWizard.Shared;
public class Cell(CellState cellState)
{
    public CellState CellState { get; set;} = cellState;
}