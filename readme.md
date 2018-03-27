# Entegra Systems Programming Challenge
## Building
Developed and built in Visual Studio 2017.  Open `evan_schiewe_entegra_challenge.sln` and build the solution.  2 NuGet packages (IKVM and Standford.NLP.CoreNLP) should be pulled on the first build.

## Running
After building or downloading the release, `evan_schiewe_entegra_challenge.exe` can be run in a console without arguments.  Test cases are included in Program.cs. 

The parser can be used in other applications by `using BusinessCard` and adding `BusinessCardParser.cs` and `ContactInfo.cs` to your project.  Currently, the name parsing assumes that the `english.all.3class.distsim.crf.ser.gz` model file is in the working directory.

## Notes
* Because Stanford CoreNLP's Named Entity Recognition is used to extract names, instantiating a BusinessCardParser takes approximately 5 seconds while the model is loaded.  After this, actual parsing performance is quite fast.
* `SLF4J: Failed to load...`: This is a [known issue with the Stanford.NLP.NET package](https://github.com/sergey-tihon/Stanford.NLP.NET/issues/79), but thankfully doesn't get in the way..