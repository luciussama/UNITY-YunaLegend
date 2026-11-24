using System.Globalization;
using System.Text;
using System.Text.Json;
using YunaLegend.Picnic.Core;

internal static class JumpCsvExporter
{
    internal static void Export()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        var stage = new PicnicStageSpec();
        string directory = Path.GetFullPath("Evidence/Picnic/JumpCalibration/CandidateSurface");
        foreach (int rate in new[] { 30, 60, 120, 0 })
        {
            string input = Path.Combine(directory, $"jump-points-{rate}.json");
            if (!File.Exists(input))
            {
                continue;
            }
            using var document = JsonDocument.Parse(File.ReadAllText(input));
            var output = new StringBuilder("jump,from_index,from_id,to_index,to_id,takeoff_command_time_s,takeoff_command_x_m,takeoff_command_y_m,landing_observed_frame_time_s,landing_x_m,landing_y_m,signed_edge_gap_at_observed_times_m,moving_endpoint,same_support_hop,source_right_at_takeoff_m,destination_left_at_landing_m,destination_right_at_landing_m,takeoff_surface_height_m,landing_surface_height_m,landing_surface_minus_takeoff_surface_m,landing_left_clearance_m,landing_right_clearance_m,core_horizontal_landing_tolerance_m,within_core_horizontal_tolerance\n");
            int index = 0;
            int negativeMargins = 0;
            int outsideTolerance = 0;
            float minimumMargin = float.PositiveInfinity;
            foreach (var jump in document.RootElement.EnumerateArray())
            {
                index++;
                int from = jump.GetProperty("from").GetInt32();
                int to = jump.GetProperty("to").GetInt32();
                float takeoffTime = jump.GetProperty("takeoffTime").GetSingle();
                float takeoffX = jump.GetProperty("takeoffX").GetSingle();
                float landingTime = jump.GetProperty("landingTime").GetSingle();
                float landingX = jump.GetProperty("landingX").GetSingle();
                var source = stage.Supports[from];
                var destination = stage.Supports[to];
                float sourceRight = source.Right(takeoffTime);
                float left = destination.Left(landingTime);
                float right = destination.Right(landingTime);
                float takeoffSurface = source.Height(takeoffTime, takeoffX);
                float landingSurface = destination.Height(landingTime, landingX);
                float marginLeft = landingX - left;
                float marginRight = right - landingX;
                float margin = Math.Min(marginLeft, marginRight);
                bool inTolerance = marginLeft >= -0.08f && marginRight >= -0.08f;
                minimumMargin = Math.Min(minimumMargin, margin);
                if (margin < 0f)
                {
                    negativeMargins++;
                }
                if (!inTolerance)
                {
                    outsideTolerance++;
                }
                bool moving = source.Wheel >= 0 || destination.Wheel >= 0;
                output.AppendLine(string.Join(",", new[]
                {
                    index.ToString(), from.ToString(), source.Id, to.ToString(), destination.Id,
                    jump.GetProperty("takeoffTime").GetRawText(), jump.GetProperty("takeoffX").GetRawText(),
                    jump.GetProperty("takeoffY").GetRawText(), jump.GetProperty("landingTime").GetRawText(),
                    jump.GetProperty("landingX").GetRawText(), jump.GetProperty("landingY").GetRawText(),
                    jump.GetProperty("gap").GetRawText(), moving.ToString(), (from == to).ToString(),
                    sourceRight.ToString("R"), left.ToString("R"), right.ToString("R"),
                    takeoffSurface.ToString("R"), landingSurface.ToString("R"),
                    (landingSurface - takeoffSurface).ToString("R"), marginLeft.ToString("R"),
                    marginRight.ToString("R"), "0.08", inTolerance.ToString()
                }));
            }
            File.WriteAllText(Path.Combine(directory, $"jump-points-{rate}.csv"), output.ToString());
            Console.WriteLine($"rate={rate};jumps={index};negativeMarginRows={negativeMargins};outsideCoreToleranceRows={outsideTolerance};minimumMargin_m={minimumMargin:R};coreTolerance_m=0.08");
        }
    }
}
