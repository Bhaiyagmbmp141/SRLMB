namespace SRLMB.Civil3D.Models
{
    internal class StructureData
    {
        public string Network          { get; set; } = "";
        public string Name             { get; set; } = "";
        public string Description      { get; set; } = "";
        public double RimElevation     { get; set; } // metres
        public double SumpElevation    { get; set; } // metres
        public double InnerDiameterMm  { get; set; } // millimetres
        public int    ConnectedPipes   { get; set; }
    }
}
