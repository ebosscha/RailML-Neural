using RailMLNeural.Data;
using RailMLNeural.Neural.Normalization;
using RailMLNeural.Neural.PreProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data
{
    public interface IRecurrentDataProvider
    {
        bool IsInput { get; }
        string Name { get; }
        int Size { get; }
        int StartIndex { get; }
        double[] Process(EdgeTrainRepresentation rep);
        NormalizationTypeEnum NormalizationType { get; }
        List<string> Map { get; }
        void Update(double[] output, EdgeTrainRepresentation rep);
        
    }

    [Serializable]
    public abstract class BaseRecurrentDataProvider
    {
        public virtual void Update(double[] output, EdgeTrainRepresentation rep)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public enum RecurrentDataProviderEnum
    {
        PreviousDelaySizeInputRecurrentProvider,
        DelaySizeInputRecurrentProvider,
        ScheduledTimesInputRecurrentProvider,
        ForecastedTimesInputRecurrentProvider,
        DelaySizeOutputRecurrentProvider,
        IsDelayedOutputRecurrentProvider,
        TrainSpeedInputRecurrentProvider,
        EdgeMaxSpeedInputRecurrentProvider,
        EdgeSpeedHomogeneityInputRecurrentProvider,
        EdgeHeadwayInputRecurrentProvider,
        EdgeDelaysInputRecurrentProvider,
        VerticesHeadwayInputRecurrentProvider,
        RouteLengthInputRecurrentProvider,
        EdgeDepartureCountInputRecurrentProvider,
        VertexDepartureCountInputRecurrentProvider,
        StationTracksInputRecurrentProvider,
        DoubleTrackInputRecurrentProvider,
        SwitchCountInputRecurrentProvider,
        TrainDirectionInputRecurrentProvider,
        TotalDelayOutputRecurrentProvider,
        RelativeTotalDelayOutputRecurrentProvider,
        InclusiveTotalDelayOutputRecurrentProvider,
        TotalDelayClassificationProvider,
        InclusiveTotalDelayClassificationProvider,
        
    }
}
