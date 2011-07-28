using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace TurnOut.VersionUpdate.Tools
{
    /// <summary>
    /// Arguments class
    /// </summary>

    public class Arguments
    {
        // Variables
        private StringDictionary _dicParameters;

        // Constructor
        public Arguments(string[] args)
        {
            _dicParameters = new StringDictionary();
            Regex Spliter = new Regex(@"^-{1,2}|^/|=|:",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            Regex Remover = new Regex(@"^['""]?(.*?)['""]?$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            string sParameter = null;
            string[] arParts;

            // Valid parameters forms:
            // {-,/,--}param{ ,=,:}((",')value(",'))
            // Examples: 
            // -param1 value1 --param2 /param3:"Test-:-work" 
            //   /param4=happy -param5 '--=nice=--'

            foreach (string Txt in args)
            {
                // Look for new parameters (-,/ or --) and a
                // possible enclosed value (=,:)

                arParts = Spliter.Split(Txt, 3);

                switch (arParts.Length)
                {
                    // Found a value (for the last parameter
                    // found (space separator))

                    case 1:
                        if (sParameter != null)
                        {
                            if (!_dicParameters.ContainsKey(sParameter))
                            {
                                arParts[0] =
                                    Remover.Replace(arParts[0], "$1");

                                _dicParameters.Add(sParameter, arParts[0]);
                            }
                            sParameter = null;
                        }
                        // else Error: no parameter waiting for a value (skipped)

                        break;

                    // Found just a parameter

                    case 2:
                        // The last parameter is still waiting. 

                        // With no value, set it to true.

                        if (sParameter != null)
                        {
                            if (!_dicParameters.ContainsKey(sParameter))
                                _dicParameters.Add(sParameter, "true");
                        }
                        sParameter = arParts[1];
                        break;

                    // Parameter with enclosed value

                    case 3:
                        // The last parameter is still waiting. 

                        // With no value, set it to true.

                        if (sParameter != null)
                        {
                            if (!_dicParameters.ContainsKey(sParameter))
                                _dicParameters.Add(sParameter, "true");
                        }

                        sParameter = arParts[1];

                        // Remove possible enclosing characters (",')

                        if (!_dicParameters.ContainsKey(sParameter))
                        {
                            arParts[2] = Remover.Replace(arParts[2], "$1");
                            _dicParameters.Add(sParameter, arParts[2]);
                        }

                        sParameter = null;
                        break;
                }
            }
            // In case a parameter is still waiting

            if (sParameter != null)
            {
                if (!_dicParameters.ContainsKey(sParameter))
                    _dicParameters.Add(sParameter, "true");
            }
        }

        // Retrieve a parameter value if it exists
        // (overriding C# indexer property)
        public string this[string param]
        {
            get
            {
                return (_dicParameters[param]);
            }
        }
    }
}