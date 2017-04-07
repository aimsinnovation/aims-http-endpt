using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using WixToolset.Dtf.WindowsInstaller;
using Env = System.Environment;

namespace Aims.Sdk.Installer.Actions
{
    public class ParameterActions
    {
        [CustomAction]
        public static ActionResult ValidatePaths(Session session)
        {
            try
            {
                session.Log("Begin ValidateEndpoints");

                string[] endpoints = session["AIMS_ENDPOINTS_MULTILINE"]
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim(' ', '\t'))
                    .Select(p => p.Trim('"'))
                    .Where(p => !String.IsNullOrWhiteSpace(p))
                    .ToArray();
                string[] invalidPaths = endpoints
                    .Where(path =>
                    {
                        Uri uri;
                        return !Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out uri);
                    })
                    .ToArray();

                if (invalidPaths.Any())
                {
                    MessageBox.Show(String.Join(Env.NewLine,
                        new[] { "The specified HTTP endpoint has wrong format:" }.Concat(invalidPaths)),
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    session["AIMS_ENDPOINTS_VALID"] = "0";
                    return ActionResult.Success;
                }

                session["AIMS_ENDPOINTS"] = String.Join(";", endpoints.Select(p => "\"" + p + "\""));
                session["AIMS_ENDPOINTS_VALID"] = "1";
                
                session.Log("End ValidateEndpoints");
                return ActionResult.Success;
            }
            catch (Exception ex)
            {
                session.Log("ValidateEndpoints, exception: {0}", ex.Message);
                return ActionResult.Failure;
            }
        }
    }
}