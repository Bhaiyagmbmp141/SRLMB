namespace SRLMB.Civil3D.Models
{
    internal class PipeData
    {
        public string Network           { get; set; } = "";
        public string Name              { get; set; } = "";
        public string Description       { get; set; } = "";
        public double Length2D          { get; set; }
        public double Length3D          { get; set; }
        public double InnerDiameterMm   { get; set; } // millimetres
        public double SlopePct          { get; set; } // percent
        public double StartInvert       { get; set; } // metres
        public double EndInvert         { get; set; } // metres
        public string Material          { get; set; } = "";
        public string StartStructure    { get; set; } = "";
        public string EndStructure      { get; set; } = "";
        public double FullFlowCapLs     { get; set; } // litres/sec (Manning's full-flow)
        public string Status            { get; set; } = ""; // OK / WARN / FAIL
    }
}
