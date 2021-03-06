// <copyright file="TemplateManifest.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AudioAnalysisTools.ContentDescriptionTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Templates are initially defined manually in a YAML file. Each template in a YAML file is called a "manifest".
    /// The array of manifests in a yml file is used to calculate an array of "functional templates" in a json file.
    /// The json file is generated automatically from the information provided in the manifests.yml file.
    /// A  template manifest contains the "provenance" of the template (i.e. details of the recordings, source locations etc used to make the functional template.
    /// It also contains the information required to calculate the template definition.
    /// The core of a functional template is its definition, which is stored as a dictionary of spectral indices.
    /// The functional template also contains information required to scan new recordings with the template definition.
    ///
    /// Each template manifest in a yml file contains an EditStatus field which describes what to with the manifest.
    /// There are three options as described below.
    /// </summary>
    public enum EditStatus
    {
        Edit,   // This option edits an existing functional template in the json file. The template definition is (re)calculated.
        Copy,   // This option keeps an existing functional template unchanged.
        Ignore, // This option ignores the manifest, i.e. does not transfer it to the array of functional templates.
    }

    /// <summary>
    /// This is base class for both template manifests and functional templates.
    /// Most of the fields and properties are common to both manifests and functional templates.
    /// Manifests contain the template provenance. This does not appear in the functional template because provenance includes path data.
    /// This class also contains methods to create new or edit existing functional templates based on info in the manifests.
    /// </summary>
    public class TemplateManifest
    {
        /// <summary>
        /// This method calculates new template based on passed manifest.
        /// </summary>
        public static Dictionary<string, double[]> CreateTemplateDefinition(TemplateManifest templateManifest)
        {
            // Get the template provenance. Assume array contains only one element.
            var provenanceArray = templateManifest.Provenance;
            var provenance = provenanceArray[0];
            var sourceDirectory = provenance.Directory;
            var baseName = provenance.Basename;

            // Read all indices from the complete recording. The path variable is a partial path requiring to be appended.
            var path = Path.Combine(sourceDirectory, baseName + ContentSignatures.AnalysisString);
            var dictionaryOfIndices = DataProcessing.ReadIndexMatrices(path);
            var algorithmType = templateManifest.FeatureExtractionAlgorithm;
            Dictionary<string, double[]> newTemplateDefinition;

            switch (algorithmType)
            {
                case 1:
                    newTemplateDefinition = ContentAlgorithms.CreateFullBandTemplate1(templateManifest, dictionaryOfIndices);
                    break;
                case 2:
                    newTemplateDefinition = ContentAlgorithms.CreateBroadbandTemplate1(templateManifest, dictionaryOfIndices);
                    break;
                case 3:
                    newTemplateDefinition = ContentAlgorithms.CreateNarrowBandTemplate1(templateManifest, dictionaryOfIndices);
                    break;
                default:
                    //LoggedConsole.WriteWarnLine("Algorithm " + algorithmType + " does not exist.");
                    newTemplateDefinition = null;
                    break;
            }

            return newTemplateDefinition;
        }

        //TEMPLATE DESCRIPTION
        // Name of the template
        public string Name { get; set; }

        //TEMPLATE DESCRIPTION
        // Name of the template
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a comment about the template.
        /// e.g. "Detects light rain".
        /// </summary>
        public string GeneralComment { get; set; }

        /// <summary>
        /// Gets or sets the template edit status.
        /// EditStatus can be "locked", etc.
        /// </summary>
        public EditStatus EditStatus { get; set; }

        //ALGORITHMIC PARAMETERS ASSOCIATED WITH TEMPLATE
        public byte FeatureExtractionAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets the factor by which a spectrum of index values is reduced.
        /// Full array (256 freq bins) of spectral indices is reduced by the following factor by averaging.
        /// This is to reduce correlation and computation.
        /// </summary>
        public int SpectralReductionFactor { get; set; }

        /// <summary>
        /// Gets or sets the bottom freq of bandpass filter.
        /// Bandpass filter to be applied where the target content exists only within a narrow band, e.g. 3-4 kHz for Silver-eye band.
        /// Bottom of the required frequency band.
        /// </summary>
        public int BandMinHz { get; set; }

        /// <summary>
        /// Gets or sets the top freq of bandpass filter.
        /// Bandpass filter to be applied where the target content exists only within a narrow band, e.g. 3-4 kHz for Silver-eye band.
        /// Top of the required frequency band.
        /// </summary>
        public int BandMaxHz { get; set; }

        public SourceAudioProvenance[] Provenance { get; set; }
    }

    /// <summary>
    /// This class holds info about provenance of a recording used to construct a template.
    /// </summary>
    public class SourceAudioProvenance
    {
        /*
        // TODO: use this property as the source audio for which indices will be calculated from
        /// <summary>
        /// Gets or sets the template Recording Location.
        /// </summary>
        public string Path { get; set; }
        */

        // TODO: remove when calculating indices directly from audio segments
        /// <summary>
        /// Gets or sets the directory containing the source index files".
        /// </summary>
        public string Directory { get; set; }

        // TODO: remove when calculating indices directly from audio segments
        /// <summary>
        /// Gets or sets the basename for the source index files".
        /// Gets or sets the first minute (or matrix row assuming one-minute per row) of the selected indices.
        /// The rows/minutes are inclusive.
        /// </summary>
        public string Basename { get; set; }

        /// <summary>
        /// Gets or sets the template Recording Location".
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the first minute (or matrix row assuming one-minute per row) of the selected indices.
        /// The rows/minutes are inclusive.
        /// </summary>
        public int StartOffset { get; set; }

        public int EndOffset { get; set; }
    }

    public class FunctionalTemplate
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionalTemplate"/> class.
        /// CONSTRUCTOR must initialise the info from the Manifest.
        /// </summary>
        /// <param name="templateManifest">The template manifest.</param>
        public FunctionalTemplate(TemplateManifest templateManifest) => this.Manifest = templateManifest;

        public TemplateManifest Manifest { get; set; }

        /// <summary>
        /// Gets or sets the date the functional template was created.
        /// </summary>
        public DateTimeOffset MostRecentEdit { get; set; }

        public Dictionary<string, double[]> Template { get; set; }
    }
}
