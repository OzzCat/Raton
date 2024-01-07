namespace Raton.Tables.Models
{
    public interface ITableModel
    {
        public int TableID { get; }
        public bool IsDirty { get; set; }
    }
}
