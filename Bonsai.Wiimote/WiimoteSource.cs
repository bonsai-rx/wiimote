using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using WiimoteLib;

namespace Bonsai.Wiimote
{
    public class WiimoteSource : Source<WiimoteState>
    {
        public WiimoteSource()
        {
            ReportType = InputReport.ButtonsAccel;
            Sensitivity = IRSensitivity.Maximum;
            Continuous = true;
        }

        public int Index { get; set; }

        public InputReport ReportType { get; set; }

        public IRSensitivity Sensitivity { get; set; }

        public bool Continuous { get; set; }

        public override IObservable<WiimoteState> Generate()
        {
            return Observable.Defer(() =>
            {
                var index = Index;
                if (index < 0 || index > 4)
                {
                    throw new InvalidOperationException("Wiimote index must be between zero and three.");
                }

                var wiimotes = new WiimoteCollection();
                wiimotes.FindAllWiimotes();
                if (index >= wiimotes.Count)
                {
                    throw new InvalidOperationException("No Wiimote with the specified index found.");
                }

                var wiimote = wiimotes[index];
                wiimote.Connect();
                wiimote.SetLEDs(index);
                wiimote.SetReportType(ReportType, Sensitivity, Continuous);
                return Observable.FromEventPattern<WiimoteChangedEventArgs>(
                    handler => wiimote.WiimoteChanged += handler,
                    handler => wiimote.WiimoteChanged -= handler)
                    .Select(evt => evt.EventArgs.WiimoteState)
                    .Finally(() =>
                    {
                        wiimote.SetLEDs(0);
                        wiimote.Disconnect();
                        wiimote.Dispose();
                    });
            });
        }
    }
}
