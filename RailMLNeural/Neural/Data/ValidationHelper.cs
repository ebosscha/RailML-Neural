using Encog.ML.Data;
using RailMLNeural.Neural.Algorithms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RailMLNeural.Neural.Data
{
    class ValidationHelper
    {
        private List<IMLData> _outputs;
        private List<IMLData> _ideals;
        private MSEErrorCalculation msecalc;
        private WeightedMSEErrorCalculation nmsecalc;
        private int _count;
        private int _outputsize;

        public ValidationHelper(int outputsize)
        {
            _outputs = new List<IMLData>();
            _ideals = new List<IMLData>();
            msecalc = new MSEErrorCalculation();
            nmsecalc = new WeightedMSEErrorCalculation(outputsize);
            _outputsize = outputsize;

        }

        public void Add(IMLData Output, IMLData Ideal)
        {
            _outputs.Add(Output);
            _ideals.Add(Ideal);
            msecalc.UpdateError(Output, Ideal, 1.0);
            nmsecalc.UpdateError(Output, Ideal, 1.0);
            _count++;
        }

        public string PublishMSE()
        {
            string msg = "Verification DelayCombination Count : " + _count +
                "\n MSE : " + msecalc.CalculateError() + "\n NMSE : " + nmsecalc.CalculateError() +
                "\n Rsquared : " + (1 - nmsecalc.CalculateError());
            return msg;
        }

        public void SaveHistogram()
        {
            var nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            string _hist = "";
            for(int i = 0; i < _outputsize; i++)
            {
                _hist += "x" + " y " + "label" + Environment.NewLine;
            }
            for(int i = 0; i < _count; i++)
            {
                for(int j = 0; j < _outputsize; j++)
                {
                    _hist += _ideals[i][j].ToString(nfi) + " " + _outputs[i][j].ToString(nfi) +" " + j + Environment.NewLine;
                }
            }

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.AddExtension = true;
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Text Document (.txt.)|*.txt"; // Filter files by extension

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document

                string filename = dlg.FileName;
                System.IO.File.WriteAllText(filename, _hist);
            }
        }

    }
}
