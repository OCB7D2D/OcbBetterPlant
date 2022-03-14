using System.Collections.Generic;
using UnityEngine;

public class GrassShadowCmd : ConsoleCmdAbstract
{

    private static string info = "BetterPlants";

    public override string[] GetCommands()
    {
        return new string[2] { info, "plants" };
    }

    public override string GetDescription() => "Plant Settings";

    public override string GetHelp() => "Fine tune how plants are rendered\n";

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {

        if (_params.Count == 1)
        {
            switch (_params[0])
            {
                case "off":
                    OcbBetterPlants.Enabled = false;
                    break;
                case "on":
                    OcbBetterPlants.Enabled = true;
                    break;
                default:
                    Log.Warning("Unknown command " + _params[0]);
                    break;
            }
        }

        else if (_params.Count == 2)
        {
            switch (_params[0])
            {
                case "RotX":
                    OcbBetterPlants.RotX = float.Parse(_params[1]);
                    break;
                case "RotY":
                    OcbBetterPlants.RotY = float.Parse(_params[1]);
                    break;
                case "RotZ":
                    OcbBetterPlants.RotZ = float.Parse(_params[1]);
                    break;
                case "ScaleMin":
                    OcbBetterPlants.ScaleMin = float.Parse(_params[1]);
                    break;
                case "ScaleMax":
                    OcbBetterPlants.ScaleMax = float.Parse(_params[1]);
                    break;
                default:
                    Log.Warning("Unknown command " + _params[0]);
                    break;
            }
        }
    }

}
