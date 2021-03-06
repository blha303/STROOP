﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Schema;
using System.IO;
using System.Reflection;
using SM64_Diagnostic.Structs;
using System.Drawing;
using System.Windows.Forms;
using SM64_Diagnostic.Extensions;
using System.Xml;
using System.Net;

namespace SM64_Diagnostic.Utilities
{
    public static class XmlConfigParser
    {
        public class ResourceXmlResolver : XmlResolver
        {
            /// <summary>
            /// When overridden in a derived class, maps a URI to an object containing the actual resource.
            /// </summary>
            /// <returns>
            /// A System.IO.Stream object or null if a type other than stream is specified.
            /// </returns>
            /// <param name="absoluteUri">The URI returned from <see cref="M:System.Xml.XmlResolver.ResolveUri(System.Uri,System.String)"/>. </param><param name="role">The current version does not use this parameter when resolving URIs. This is provided for future extensibility purposes. For example, this can be mapped to the xlink:role and used as an implementation specific argument in other scenarios. </param><param name="ofObjectToReturn">The type of object to return. The current version only returns System.IO.Stream objects. </param><exception cref="T:System.Xml.XmlException"><paramref name="ofObjectToReturn"/> is not a Stream type. </exception><exception cref="T:System.UriFormatException">The specified URI is not an absolute URI. </exception><exception cref="T:System.ArgumentNullException"><paramref name="absoluteUri"/> is null. </exception><exception cref="T:System.Exception">There is a runtime error (for example, an interrupted server connection). </exception>
            public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
            {
                // If ofObjectToReturn is null, then any of the following types can be returned for correct processing:
                // Stream, TextReader, XmlReader or descendants of XmlSchema
                var result = this.GetType().Assembly.GetManifestResourceStream(
                    string.Format("SM64_Diagnostic.Schemas.{0}", Path.GetFileName(absoluteUri.ToString())));

                // set a conditional breakpoint "result==null" here
                return result;
            }
        }

        public static void OpenConfig(string path)
        {
            Config.ObjectSlots = new ObjectSlotsConfig();
            Config.ObjectGroups = new ObjectGroupsConfig();
            Config.ObjectGroups.ProcessingGroups = new List<byte>();
            Config.ObjectGroups.ProcessingGroupsColor = new Dictionary<byte, Color>();
            var assembly = Assembly.GetExecutingAssembly();

            // Create schema set
            var schemaSet = new XmlSchemaSet() { XmlResolver = new ResourceXmlResolver() };
            schemaSet.Add("http://tempuri.org/ReusableTypes.xsd", "ReusableTypes.xsd");
            schemaSet.Add("http://tempuri.org/ConfigSchema.xsd", "ConfigSchema.xsd");
            schemaSet.Compile();

            // Load and validate document
            var doc = XDocument.Load(path);
            doc.Validate(schemaSet, Validation);

            foreach(XElement element in doc.Root.Elements())
            {
                switch(element.Name.ToString())
                {
                    case "RefreshRateFreq":
                        Config.RefreshRateFreq = int.Parse(element.Value);
                        break;

                    case "ProcessDefaultName":
                        Config.ProcessName = element.Value;
                        break;

                    case "RAMStartAddress":
                        Config.RamStartAddress = ParsingUtilities.ParseHex(element.Value);
                        break;

                    case "RAMSize":
                        Config.RamSize = ParsingUtilities.ParseHex(element.Value);
                        break;
                    case "ObjectSlots":
                        foreach (XElement subElement in element.Elements())
                        {
                            switch (subElement.Name.ToString())
                            {
                                case "FirstObjectAddress":
                                    Config.ObjectSlots.LinkStartAddress = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "ObjectStructSize":
                                    Config.ObjectSlots.StructSize = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "HeaderOffset":
                                    Config.ObjectSlots.HeaderOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "ListNextLinkOffset":
                                    Config.ObjectSlots.NextLinkOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "ListPreviousLinkOffset":
                                    Config.ObjectSlots.PreviousLinkOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "BehaviorScriptOffset":
                                    Config.ObjectSlots.BehaviorScriptOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "BehaviorGfxOffset":
                                    Config.ObjectSlots.BehaviorGfxOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "BehaviorSubtypeOffset":
                                    Config.ObjectSlots.BehaviorSubtypeOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "BehaviorAppearance":
                                    Config.ObjectSlots.BehaviorAppearance = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "ObjectActiveOffset":
                                    Config.ObjectSlots.ObjectActiveOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "CoordinateOffsetX":
                                    Config.ObjectSlots.ObjectXOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "CoordinateOffsetY":
                                    Config.ObjectSlots.ObjectYOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "CoordinateOffsetZ":
                                    Config.ObjectSlots.ObjectZOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "HomeOffsetX":
                                    Config.ObjectSlots.HomeXOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "HomeOffsetY":
                                    Config.ObjectSlots.HomeYOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "HomeOffsetZ":
                                    Config.ObjectSlots.HomeZOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "RotationOffset":
                                    Config.ObjectSlots.ObjectRotationOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "MoveToMarioYOffset":
                                    Config.ObjectSlots.MoveToMarioYOffset = float.Parse(subElement.Value);
                                    break;
                                case "MaxObjectSlots":
                                    Config.ObjectSlots.MaxSlots = int.Parse(subElement.Value);
                                    break;
                            }
                        }
                        break;
               
                    case "ObjectGroups":
                        foreach (XElement subElement in element.Elements())
                        {
                            switch (subElement.Name.ToString())
                            {
                                case "ProcessNextLinkOffset":
                                    Config.ObjectGroups.ProcessNextLinkOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "ProcessPreviousLinkOffset":
                                    Config.ObjectGroups.ProcessPreviousLinkOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "ParentObjectOffset":
                                    Config.ObjectGroups.ParentObjectOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "FirstObjectGroupingAddress":
                                    Config.ObjectGroups.FirstGroupingAddress = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "VacantPointerAddress":
                                    Config.ObjectGroups.VactantPointerAddress = ParsingUtilities.ParseHex(subElement.Value);
                                    Config.ObjectGroups.VacantSlotColor = ColorTranslator.FromHtml(subElement.Attribute(XName.Get("color")).Value);
                                    break;
                                case "ProcessGroupStructSize":
                                    Config.ObjectGroups.ProcessGroupStructSize = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "ProcessGroupsOrdering":
                                    foreach(var subSubElement in subElement.Elements())
                                    {
                                        var group = (byte)ParsingUtilities.ParseHex(
                                            subSubElement.Attribute(XName.Get("index")).Value);
                                        var color = ColorTranslator.FromHtml(
                                            subSubElement.Attribute(XName.Get("color")).Value);

                                        Config.ObjectGroups.ProcessingGroups.Add(group);
                                        Config.ObjectGroups.ProcessingGroupsColor.Add(group,color);
                                    }
                                    break;
                            }
                        }
                        break;

                    case "Mario":
                        foreach (XElement subElement in element.Elements())
                        {
                            switch (subElement.Name.ToString())
                            {
                                case "MarioStructAddress":
                                    Config.Mario.StructAddress = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "CoordinateOffsetX":
                                    Config.Mario.XOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "CoordinateOffsetY":
                                    Config.Mario.YOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "CoordinateOffsetZ":
                                    Config.Mario.ZOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "FacingAngleOffset":
                                    Config.Mario.RotationOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "MarioStructSize":
                                    Config.Mario.StructSize = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "ActionOffset":
                                    Config.Mario.ActionOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "MoveToObjectYOffset":
                                    Config.Mario.MoveToObjectYOffset = float.Parse(subElement.Value);
                                    break;
                                case "StandingOnPointer":
                                    Config.Mario.StandingOnObjectPointer = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "InteractingObjectPointerOffset":
                                    Config.Mario.InteractingObjectPointerOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break; 
                                case "HoldingObjectPointerOffset":
                                    Config.Mario.HoldingObjectPointerOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "UsingObjectPointerOffset":
                                    Config.Mario.UsingObjectPointerOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "CeilingYOffset":
                                    Config.Mario.CeilingYOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "GroundYOffset":
                                    Config.Mario.GroundYOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "HSpeedOffset":
                                    Config.Mario.HSpeedOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "FloorTriangleOffset":
                                    Config.Mario.FloorTriangleOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "WallTriangleOffset":
                                    Config.Mario.WallTriangleOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "CeilingTriangleOffset":
                                    Config.Mario.CeilingTriangleOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "SlidingSpeedXOffset":
                                    Config.Mario.SlidingSpeedXOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "SlidingSpeedZOffset":
                                    Config.Mario.SlidingSpeedZOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "PeakHeightOffset":
                                    Config.Mario.PeakHeightOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                            }
                        }
                        break;

                    case "Hud":
                        foreach (XElement subElement in element.Elements())
                        {
                            switch (subElement.Name.ToString())
                            {
                                case "HpAddress":
                                    Config.Hud.HpAddress = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "LiveCountAddress":
                                    Config.Hud.LiveCountAddress = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "CoinCountAddress":
                                    Config.Hud.CoinCountAddress = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "StarCountAddress":
                                    Config.Hud.StarCountAddress = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "FullHp":
                                    Config.Hud.FullHp = short.Parse(subElement.Value);
                                    break;
                                case "StandardLives":
                                    Config.Hud.StandardLives = short.Parse(subElement.Value);
                                    break;
                                case "StandardCoins":
                                    Config.Hud.StandardCoins = short.Parse(subElement.Value);
                                    break;
                                case "StandardStars":
                                    Config.Hud.StandardStars = short.Parse(subElement.Value);
                                    break;
                            }
                        }
                        break;

                    case "Debug":
                        foreach (XElement subElement in element.Elements())
                        {
                            switch (subElement.Name.ToString())
                            {
                                case "ToggleAddress":
                                    Config.Debug.Toggle = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "SettingAddress":
                                    Config.Debug.Setting = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                            }
                        }
                        break;

                    case "TriangleOffsets":
                        foreach (XElement subElement in element.Elements())
                        {
                            switch (subElement.Name.ToString())
                            {
                                case "surfaceType":
                                    Config.TriangleOffsets.SurfaceType = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "flags":
                                    Config.TriangleOffsets.Flags = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "windDirection":
                                    Config.TriangleOffsets.WindDirection = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "wallProjection":
                                    Config.TriangleOffsets.WallProjection = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "yMin":
                                    Config.TriangleOffsets.YMin = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "yMax":
                                    Config.TriangleOffsets.YMax = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "x1":
                                    Config.TriangleOffsets.X1 = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "y1":
                                    Config.TriangleOffsets.Y1 = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "z1":
                                    Config.TriangleOffsets.Z1 = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "x2":
                                    Config.TriangleOffsets.X2 = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "y2":
                                    Config.TriangleOffsets.Y2 = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "z2":
                                    Config.TriangleOffsets.Z2 = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "x3":
                                    Config.TriangleOffsets.X3 = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "y3":
                                    Config.TriangleOffsets.Y3 = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "z3":
                                    Config.TriangleOffsets.Z3 = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "normX":
                                    Config.TriangleOffsets.NormX = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "normY":
                                    Config.TriangleOffsets.NormY = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "normZ":
                                    Config.TriangleOffsets.NormZ = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "offset":
                                    Config.TriangleOffsets.Offset = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                                case "associatedObject":
                                    Config.TriangleOffsets.AssociatedObject = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                            }
                        } 
                        break;

                    case "LevelAddress":
                        Config.LevelAddress = ParsingUtilities.ParseHex(element.Value);
                        break;

                    case "AreaAddress":
                        Config.AreaAddress = ParsingUtilities.ParseHex(element.Value);
                        break;

                    case "LoadingPointAddress":
                        Config.LoadingPointAddress = ParsingUtilities.ParseHex(element.Value);
                        break;

                    case "MissionLayoutAddress":
                        Config.MissionAddress = ParsingUtilities.ParseHex(element.Value);
                        break;

                    case "HolpX":
                        Config.HolpX = ParsingUtilities.ParseHex(element.Value);
                        break;
                    case "HolpY":
                        Config.HolpY = ParsingUtilities.ParseHex(element.Value);
                        break;
                    case "HolpZ":
                        Config.HolpZ = ParsingUtilities.ParseHex(element.Value);
                        break;
                    case "CameraX":
                        Config.CameraX = ParsingUtilities.ParseHex(element.Value);
                        break;
                    case "CameraY":
                        Config.CameraY = ParsingUtilities.ParseHex(element.Value);
                        break;
                    case "CameraZ":
                        Config.CameraZ = ParsingUtilities.ParseHex(element.Value);
                        break;
                    case "CameraRot":
                        Config.CameraRot = ParsingUtilities.ParseHex(element.Value);
                        break;
                    case "RngRecordingAreaAddress":
                        Config.RngRecordingAreaAddress = ParsingUtilities.ParseHex(element.Value);
                        break;
                    case "RngAddress":
                        Config.RngAddress = ParsingUtilities.ParseHex(element.Value);
                        break;
                }
            }
        }

        public static List<WatchVariable> OpenWatchVarData(string path, string schemaFile, string specialOffsetName = null)
        {
            var objectData = new List<WatchVariable>();
            var assembly = Assembly.GetExecutingAssembly();

            // Create schema set
            var schemaSet = new XmlSchemaSet() { XmlResolver = new ResourceXmlResolver() };
            schemaSet.Add("http://tempuri.org/ReusableTypes.xsd", "ReusableTypes.xsd");
            schemaSet.Add("http://tempuri.org/CameraDataSchema.xsd", schemaFile);
            schemaSet.Compile();

            // Load and validate document
            var doc = XDocument.Load(path);
            doc.Validate(schemaSet, Validation);

            foreach (XElement element in doc.Root.Elements())
            {
                if (element.Name.ToString() != "Data")
                    continue;

                var watchVar = GetWatchVariableFromElement(element);
                if (specialOffsetName != null)
                    watchVar.OtherOffset = (element.Attribute(XName.Get(specialOffsetName)) != null) ?
                        bool.Parse(element.Attribute(XName.Get(specialOffsetName)).Value) : false;

                objectData.Add(watchVar);
            }

            return objectData;
        }

        public static ObjectAssociations OpenObjectAssoc(string path, ObjectSlotManagerGui objectSlotManagerGui)
        {
            var assoc = new ObjectAssociations();
            var assembly = Assembly.GetExecutingAssembly();

            // Create schema set
            var schemaSet = new XmlSchemaSet() { XmlResolver = new ResourceXmlResolver() };
            schemaSet.Add("http://tempuri.org/ReusableTypes.xsd", "ReusableTypes.xsd");
            schemaSet.Add("http://tempuri.org/ObjectAssociationsSchema.xsd", "ObjectAssociationsSchema.xsd");
            schemaSet.Compile();

            // Load and validate document
            var doc = XDocument.Load(path);
            doc.Validate(schemaSet, Validation);

            // Create Behavior-ImagePath list
            string defaultImagePath = "", emptyImagePath = "", imageDir = "", mapImageDir = "", overlayImageDir = "",
                marioImagePath = "", holpMapImagePath = "", hudImagePath = "", debugImagePath = "",
                miscImagePath = "", cameraImagePath = "", marioMapImagePath = "", cameraMapImagePath = "",
                selectedOverlayImagePath = "", standingOnOverlayImagePath = "", holdingOverlayImagePath = "",
                interactingOverlayImagePath = "", usingOverlayImagePath = "", closestOverlayImagePath = "";
            uint ramToBehaviorOffset = 0;
            uint marioBehavior = 0;

            foreach (XElement element in doc.Root.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "Config":
                        foreach (XElement subElement in element.Elements())
                        {
                            switch(subElement.Name.ToString())
                            {
                                case "ImageDirectory":
                                    imageDir = subElement.Value;
                                    break;
                                case "DefaultImage":
                                    defaultImagePath = subElement.Value;
                                    break;
                                case "MapImageDirectory":
                                    mapImageDir = subElement.Value;
                                    break;
                                case "OverlayImageDirectory":
                                    overlayImageDir = subElement.Value;
                                    break;
                                case "EmptyImage":
                                    emptyImagePath = subElement.Value;
                                    break;
                                case "RamToBehaviorOffset":
                                    ramToBehaviorOffset = ParsingUtilities.ParseHex(subElement.Value);
                                    assoc.RamOffset = ramToBehaviorOffset;
                                    break;
                            }
                        }
                        break;

                    case "Mario":
                        marioImagePath = element.Element(XName.Get("Image")).Attribute(XName.Get("path")).Value;
                        marioMapImagePath = element.Element(XName.Get("MapImage")) != null ?
                            element.Element(XName.Get("MapImage")).Attribute(XName.Get("path")).Value : null;
                        assoc.MarioColor = ColorTranslator.FromHtml(element.Element(XName.Get("Color")).Value);
                        marioBehavior = ParsingUtilities.ParseHex(element.Attribute(XName.Get("behaviorScriptAddress")).Value);
                        break;

                    case "Hud":
                        hudImagePath = element.Element(XName.Get("Image")).Attribute(XName.Get("path")).Value;
                        assoc.HudColor = ColorTranslator.FromHtml(element.Element(XName.Get("Color")).Value);
                            break;

                    case "Debug":
                        debugImagePath = element.Element(XName.Get("Image")).Attribute(XName.Get("path")).Value;
                        assoc.DebugColor = ColorTranslator.FromHtml(element.Element(XName.Get("Color")).Value);
                        break;

                    case "Misc":
                        miscImagePath = element.Element(XName.Get("Image")).Attribute(XName.Get("path")).Value;
                        assoc.MiscColor = ColorTranslator.FromHtml(element.Element(XName.Get("Color")).Value);
                        break;

                    case "Camera":
                        cameraImagePath = element.Element(XName.Get("Image")).Attribute(XName.Get("path")).Value;
                        assoc.CameraColor = ColorTranslator.FromHtml(element.Element(XName.Get("Color")).Value);
                        cameraMapImagePath = element.Element(XName.Get("MapImage")).Attribute(XName.Get("path")).Value;
                        break;

                    case "Holp":
                        holpMapImagePath = element.Element(XName.Get("MapImage")).Attribute(XName.Get("path")).Value;
                        break;

                    case "Overlays":
                        foreach (XElement subElement in element.Elements())
                        {
                            switch (subElement.Name.ToString())
                            {
                                case "Selected":
                                    selectedOverlayImagePath = subElement.Element(XName.Get("OverlayImage")).Attribute(XName.Get("path")).Value;
                                    break;

                                case "StandingOn":
                                    standingOnOverlayImagePath = subElement.Element(XName.Get("OverlayImage")).Attribute(XName.Get("path")).Value;
                                    break;

                                case "Holding":
                                    holdingOverlayImagePath = subElement.Element(XName.Get("OverlayImage")).Attribute(XName.Get("path")).Value;
                                    break;

                                case "Interacting":
                                    interactingOverlayImagePath = subElement.Element(XName.Get("OverlayImage")).Attribute(XName.Get("path")).Value;
                                    break;

                                case "Using":
                                    usingOverlayImagePath = subElement.Element(XName.Get("OverlayImage")).Attribute(XName.Get("path")).Value;
                                    break;
                                    
                                case "Closest":
                                    closestOverlayImagePath = subElement.Element(XName.Get("OverlayImage")).Attribute(XName.Get("path")).Value;
                                    break;
                            }
                        }
                        break;

                    case "Object":
                        uint behaviorAddress = (ParsingUtilities.ParseHex(element.Attribute(XName.Get("behaviorScriptAddress")).Value)
                            - ramToBehaviorOffset) & 0x00FFFFFF;
                        uint? gfxId = null;
                        int? subType = null, appearance = null;
                        if (element.Attribute(XName.Get("gfxId")) != null)
                            gfxId = ParsingUtilities.ParseHex(element.Attribute(XName.Get("gfxId")).Value) | 0x80000000U;
                        if (element.Attribute(XName.Get("subType")) != null)
                            subType = ParsingUtilities.TryParseInt(element.Attribute(XName.Get("subType")).Value);
                        if (element.Attribute(XName.Get("appearance")) != null)
                            appearance = ParsingUtilities.TryParseInt(element.Attribute(XName.Get("appearance")).Value);
                        string imagePath = element.Element(XName.Get("Image")).Attribute(XName.Get("path")).Value;
                        string mapImagePath = null;
                        bool rotates = false;
                        if (element.Element(XName.Get("MapImage")) != null)
                        {
                            mapImagePath = element.Element(XName.Get("MapImage")).Attribute(XName.Get("path")).Value;
                            rotates = bool.Parse(element.Element(XName.Get("MapImage")).Attribute(XName.Get("rotates")).Value);
                        }
                        string name = element.Attribute(XName.Get("name")).Value;
                        var watchVars = new List<WatchVariable>();
                        foreach (var subElement in element.Elements().Where(x => x.Name == "Data"))
                        {
                            var watchVar = GetWatchVariableFromElement(subElement);
                            watchVar.OtherOffset = (subElement.Attribute(XName.Get("objectOffset")) != null) ?
                                bool.Parse(subElement.Attribute(XName.Get("objectOffset")).Value) : false;

                            watchVars.Add(watchVar);
                        }

                        var newBehavior = new ObjectBehaviorAssociation()
                        {
                            BehaviorCriteria = new BehaviorCriteria()
                            {
                                BehaviorAddress = behaviorAddress,
                                GfxId = gfxId,
                                SubType = subType,
                                Appearance = appearance
                            },
                            ImagePath = imagePath,
                            MapImagePath = mapImagePath,
                            Name = name,
                            RotatesOnMap = rotates,
                            WatchVariables  = watchVars
                        };

                        if (!assoc.AddAssociation(newBehavior))
                            throw new Exception("More than one behavior address was defined.");

                        break;
                }
            }

            // Load Images
            // TODO: Exceptions
            assoc.DefaultImage = Bitmap.FromFile(imageDir + defaultImagePath);
            assoc.EmptyImage = Bitmap.FromFile(imageDir + emptyImagePath);
            assoc.MarioImage = Bitmap.FromFile(imageDir + marioImagePath);
            assoc.CameraImage = Bitmap.FromFile(imageDir + cameraImagePath);
            assoc.MarioMapImage = marioMapImagePath == "" ? assoc.MarioImage : Bitmap.FromFile(mapImageDir + marioMapImagePath);
            assoc.HudImage = Bitmap.FromFile(imageDir + hudImagePath);
            assoc.DebugImage = Bitmap.FromFile(imageDir + debugImagePath);
            assoc.MiscImage = Bitmap.FromFile(imageDir + miscImagePath);
            assoc.HolpImage = Bitmap.FromFile(mapImageDir + holpMapImagePath);
            assoc.CameraMapImage = Bitmap.FromFile(mapImageDir + cameraMapImagePath);
            assoc.MarioBehavior = marioBehavior - ramToBehaviorOffset;
            objectSlotManagerGui.SelectedObjectOverlayImage = Bitmap.FromFile(overlayImageDir + selectedOverlayImagePath);
            objectSlotManagerGui.StandingOnObjectOverlayImage = Bitmap.FromFile(overlayImageDir + standingOnOverlayImagePath);
            objectSlotManagerGui.HoldingObjectOverlayImage = Bitmap.FromFile(overlayImageDir + holdingOverlayImagePath);
            objectSlotManagerGui.InteractingObjectOverlayImage = Bitmap.FromFile(overlayImageDir + interactingOverlayImagePath);
            objectSlotManagerGui.UsingObjectOverlayImage = Bitmap.FromFile(overlayImageDir + usingOverlayImagePath);
            objectSlotManagerGui.ClosestObjectOverlayImage = Bitmap.FromFile(overlayImageDir + closestOverlayImagePath);

            foreach (var obj in assoc.BehaviorAssociations)
            {
                if (obj.ImagePath == null || obj.ImagePath == "")
                    continue;

                using (var preLoad = Bitmap.FromFile(imageDir + obj.ImagePath))
                {
                    float scale = Math.Max(preLoad.Height / 128f, preLoad.Width / 128f);
                    obj.Image = new Bitmap(preLoad, new Size((int)(preLoad.Width / scale), (int)(preLoad.Height / scale)));
                }
                if (obj.MapImagePath == "" || obj.MapImagePath == null)
                {
                    obj.MapImage = obj.Image;
                }
                else
                {
                    using (var preLoad = Bitmap.FromFile(mapImageDir + obj.MapImagePath))
                    {
                        float scale = Math.Max(preLoad.Height / 128f, preLoad.Width / 128f);
                        obj.MapImage = new Bitmap(preLoad, new Size((int)(preLoad.Width / scale), (int)(preLoad.Height / scale)));
                    }
                }
                obj.TransparentImage = obj.Image.GetOpaqueImage(0.5f);
                obj.TransparentMapImage = obj.Image.GetOpaqueImage(0.5f);
            }

            return assoc;
        }

        public static MapAssociations OpenMapAssoc(string path)
        {
            var assoc = new MapAssociations();
            var assembly = Assembly.GetExecutingAssembly();

            // Create schema set
            var schemaSet = new XmlSchemaSet() { XmlResolver = new ResourceXmlResolver() };
            schemaSet.Add("http://tempuri.org/ReusableTypes.xsd", "ReusableTypes.xsd");
            schemaSet.Add("http://tempuri.org/MapAssociationsSchema.xsd", "MapAssociationsSchema.xsd");
            schemaSet.Compile();

            // Load and validate document
            var doc = XDocument.Load(path);
            doc.Validate(schemaSet, Validation);

            foreach (XElement element in doc.Root.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "Config":
                        foreach (XElement subElement in element.Elements())
                        {
                            switch (subElement.Name.ToString())
                            {
                                case "ImageDirectory":
                                    assoc.FolderPath = subElement.Value;
                                    break;
                                case "DefaultImage":
                                    var defaultMap = new Map() { ImagePath = subElement.Value };
                                    assoc.DefaultMap = defaultMap;
                                    break;
                                case "DefaultCoordinates":
                                    float dx1 = float.Parse(subElement.Attribute(XName.Get("x1")).Value);
                                    float dx2 = float.Parse(subElement.Attribute(XName.Get("x2")).Value);
                                    float dz1 = float.Parse(subElement.Attribute(XName.Get("z1")).Value);
                                    float dz2 = float.Parse(subElement.Attribute(XName.Get("z2")).Value);
                                    var dCoordinates = new RectangleF(dx1, dz1, dx2 - dx1, dz2 - dz1);
                                    assoc.DefaultMap.Coordinates = dCoordinates;
                                    break;
                            }
                        }
                        break;

                    case "Map":
                        byte level = byte.Parse(element.Attribute(XName.Get("level")).Value);
                        byte area = byte.Parse(element.Attribute(XName.Get("area")).Value);
                        ushort? loadingPoint = element.Attribute(XName.Get("loadingPoint")) != null ?
                            (ushort?)ushort.Parse(element.Attribute(XName.Get("loadingPoint")).Value) : null;
                        ushort? missionLayout = element.Attribute(XName.Get("missionLayout")) != null ?
                            (ushort?)ushort.Parse(element.Attribute(XName.Get("missionLayout")).Value) : null;
                        string imagePath = element.Element(XName.Get("Image")).Attribute(XName.Get("path")).Value;
                        string bgImagePath = (element.Element(XName.Get("BackgroundImage")) != null) ?
                          element.Element(XName.Get("BackgroundImage")).Attribute(XName.Get("path")).Value : null;

                        var coordinatesElement = element.Element(XName.Get("Coordinates"));
                        float x1 = float.Parse(coordinatesElement.Attribute(XName.Get("x1")).Value);
                        float x2 = float.Parse(coordinatesElement.Attribute(XName.Get("x2")).Value);
                        float z1 = float.Parse(coordinatesElement.Attribute(XName.Get("z1")).Value);
                        float z2 = float.Parse(coordinatesElement.Attribute(XName.Get("z2")).Value);
                        float y = (coordinatesElement.Attribute(XName.Get("y")) != null) ?
                            float.Parse(coordinatesElement.Attribute(XName.Get("y")).Value) : float.MinValue;

                        string name = element.Attribute(XName.Get("name")).Value;
                        string subName = (element.Attribute(XName.Get("subName")) != null) ?
                            element.Attribute(XName.Get("subName")).Value : null;

                        var coordinates = new RectangleF(x1, z1, x2 - x1, z2 - z1);

                        Map map = new Map() { Level = level, Area = area, LoadingPoint = loadingPoint, MissionLayout = missionLayout,
                            Coordinates = coordinates, ImagePath = imagePath, Y = y, Name = name, SubName = subName, BackgroundPath = bgImagePath};

                        assoc.AddAssociation(map);
                        break;
                }
            }

            return assoc;
        }

        public static ScriptParser OpenScripts(string path)
        {
            var parser = new ScriptParser();
            var assembly = Assembly.GetExecutingAssembly();

            // Create schema set
            var schemaSet = new XmlSchemaSet() { XmlResolver = new ResourceXmlResolver() };
            schemaSet.Add("http://tempuri.org/ReusableTypes.xsd", "ReusableTypes.xsd");
            schemaSet.Add("http://tempuri.org/ScriptsSchema.xsd", "ScriptsSchema.xsd");
            schemaSet.Compile();

            // Load and validate document
            var doc = XDocument.Load(path);
            doc.Validate(schemaSet, Validation);

            string scriptDir = "";
            List<Tuple<string, uint>> scriptLocations = new List<Tuple<string, uint>>();

            foreach (XElement element in doc.Root.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "Config":
                        foreach (XElement subElement in element.Elements())
                        {
                            switch (subElement.Name.ToString())
                            {
                                case "ScriptDirectory":
                                    scriptDir = subElement.Value;
                                    break;
                                case "FreeMemoryArea":
                                    parser.FreeMemoryArea = ParsingUtilities.ParseHex(subElement.Value);
                                    break;
                            }
                        }
                        break;

                    case "Script":
                        string scriptPath = element.Attribute(XName.Get("path")).Value;
                        uint insertAddress = ParsingUtilities.ParseHex(element.Attribute(XName.Get("insertAddress")).Value);
                        parser.AddScript(scriptDir + scriptPath, insertAddress, 0, 0);
                        break;
                }
            }

            return parser;
        }

        public static List<RomHack> OpenHacks(string path)
        {
            var hacks = new List<RomHack>();
            var assembly = Assembly.GetExecutingAssembly();

            // Create schema set
            var schemaSet = new XmlSchemaSet() { XmlResolver = new ResourceXmlResolver() };
            schemaSet.Add("http://tempuri.org/ScriptsSchema.xsd", "ScriptsSchema.xsd");
            schemaSet.Compile();

            // Load and validate document
            var doc = XDocument.Load(path);
            doc.Validate(schemaSet, Validation);

            string hackDir = "";

            foreach (XElement element in doc.Root.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "Config":
                        foreach (XElement subElement in element.Elements())
                        {
                            switch (subElement.Name.ToString())
                            {
                                case "HackDirectory":
                                    hackDir = subElement.Value;
                                    break;
                            }
                        }
                        break;

                    case "Hack":
                        string hackPath = hackDir + element.Attribute(XName.Get("path")).Value;
                        string name = element.Attribute(XName.Get("name")).Value;
                        hacks.Add(new RomHack(hackPath, name));
                        break;
                }
            }

            return hacks;
        }

        public static WatchVariable GetWatchVariableFromElement(XElement element)
        {
            var watchVar = new WatchVariable();
            watchVar.Special = (element.Attribute(XName.Get("special")) != null) ?
                bool.Parse(element.Attribute(XName.Get("special")).Value) : false;
            watchVar.Name = element.Value;
            watchVar.SpecialType = (element.Attribute(XName.Get("specialType")) != null) ?
                element.Attribute(XName.Get("specialType")).Value : null;
            watchVar.BackroundColor = (element.Attribute(XName.Get("color")) != null) ?
                ColorTranslator.FromHtml(element.Attribute(XName.Get("color")).Value) : (Color?)null;

            // We have fully parsed a special type
            if (watchVar.Special)
                return watchVar;

            watchVar.UseHex = (element.Attribute(XName.Get("useHex")) != null) ?
                bool.Parse(element.Attribute(XName.Get("useHex")).Value) : false;
            watchVar.AbsoluteAddressing = element.Attribute(XName.Get("absoluteAddress")) != null ?
                 bool.Parse(element.Attribute(XName.Get("absoluteAddress")).Value) : false;
            watchVar.Mask = element.Attribute(XName.Get("mask")) != null ?
                (UInt64?) ParsingUtilities.ParseExtHex(element.Attribute(XName.Get("mask")).Value) : null;
            watchVar.IsBool = element.Attribute(XName.Get("isBool")) != null ?
                bool.Parse(element.Attribute(XName.Get("isBool")).Value) : false;
            watchVar.Type = WatchVariableExtensions.GetStringType(element.Attribute(XName.Get("type")).Value);
            watchVar.Address = ParsingUtilities.ParseHex(element.Attribute(XName.Get("address")).Value);
            watchVar.InvertBool = element.Attribute(XName.Get("invertBool")) != null ?
                bool.Parse(element.Attribute(XName.Get("invertBool")).Value) : false;
            watchVar.IsAngle = element.Attribute(XName.Get("isAngle")) != null ?
                bool.Parse(element.Attribute(XName.Get("isAngle")).Value) : false;
            return watchVar;
        }

        public static void AddWatchVariableOtherData(WatchVariable watchVar)
        {
            
        }

        public static void ModifyWatchVariableOtherData(int index, WatchVariable modifiedVar)
        {

        }

        public static void DeleteWatchVariablesOtherData(List<int> indexes)
        {

        }

        private static void Validation(object sender, ValidationEventArgs e)
        {
            throw new Exception(e.Message);
        }
    }
}
