﻿using RailMLNeural.Data;
using RailMLNeural.Neural.Normalization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data.RecurrentDataProviders
{
    [Serializable]
    class TrainDirectionInputRecurrentProvider : BaseRecurrentDataProvider, IRecurrentDataProvider
    {
        public bool IsInput { get { return true; } }
        const string _name = "TrainDirectionInputRecurrentProvider";
        public String Name { get { return _name; } }
        public int Size { get { return 1; } }
        public int StartIndex { get; private set; }
        public List<string> Map { get { return new List<string>() { "Direction {1 = down, 0 = up" }; } }
        public NormalizationTypeEnum NormalizationType { get; set; }

        public TrainDirectionInputRecurrentProvider()
        {
            NormalizationType = NormalizationTypeEnum.None;
        }

        public double[] Process(EdgeTrainRepresentation rep)
        {
            double[] result = new double[Size];
            if(rep.Direction == DirectionEnum.Down)
            {
                result[0] = 1;
            }
            else
            {
                result[0] = 0;
            }
            return result;
        }

        public override void Update(double[] output, EdgeTrainRepresentation rep)
        {
            return;
        }
    }
}
