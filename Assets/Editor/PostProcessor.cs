using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
public class PostProcessor
{

#if UNITY_WEBGL
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        if (target != BuildTarget.WebGL) { return; }

        string buildPath = Path.Combine(path, "Build");
        string buildPathNew = $"Build/{Application.version}";
        string buildPathFullNew = Path.Combine(path, buildPathNew);


        foreach (var file in new DirectoryInfo(buildPath).GetDirectories())
        {
            // File.SetAttributes(file.FullName, FileAttributes.Normal);
            // File.Delete(file.FullName);
            Directory.Delete(file.FullName, true);
        }

        if (!Directory.Exists(buildPathFullNew))
        {
            Directory.CreateDirectory(buildPathFullNew);
        }

        foreach (var file in new DirectoryInfo(buildPath).GetFiles())
        {
            file.MoveTo($@"{buildPathFullNew}/{file.Name}");
        }
    }
#endif

#if UNITY_EDITOR

    [InitializeOnLoad]
    public class StartUp
    {
        static StartUp()
        {
            PlayerSettings.Android.keystorePass = "Patel.senger@14";
            PlayerSettings.Android.keyaliasPass = "Patel.senger@14";
        }
    }
#endif

#if UNITY_IOS

    // public const string BUNDLE_ID = "com.whatgames.DotTrailAdventure";
    // public const string URL_SCEME = "com.whatgames.DotTrailAdventure";

#if UNITY_CLOUD_BUILD
    public static void OnPostprocessBuildiOS (string exportPath) {
        Debug.Log ("OnPostprocessBuildiOS");
        ProcessPostBuild (BuildTarget.iOS, exportPath);
    }
#endif

    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        //if (buildTarget != BuildTarget.iPhone) { // For Unity < 5
        if (buildTarget != BuildTarget.iOS)
        {
            Debug.LogWarning("Target is not iOS. AdColonyPostProcess will not run");
            return;
        }

        //#if !UNITY_CLOUD_BUILD
        Debug.Log("OnPostprocessBuild");
        ProcessPostBuild(buildTarget, path);
        ChangeXcodePlist(buildTarget, path);
        //#endif
    }

    private static void ProcessPostBuild(BuildTarget buildTarget, string path)
    {
        string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));

        string target = proj.GetUnityMainTargetGuid();
        // proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
        // proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-fobjc-arc");
        // proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-force_load $(PROJECT_DIR)/Frameworks/Plugins/iOS/VungleSDK.framework/Versions/A/VungleSDK");

        //
        //Required Frameworks

        // proj.AddFrameworkToProject(target, "AudioToolbox.framework", false);
        // proj.AddFrameworkToProject(target, "AVFoundation.framework", false);
        // proj.AddFrameworkToProject(target, "CoreGraphics.framework", false);
        //proj.AddFrameworkToProject (target, "CoreTelephony.framework", false);
        // proj.AddFrameworkToProject(target, "CoreMedia.framework", false);
        // proj.AddFrameworkToProject(target, "EventKit.framework", false);
        // proj.AddFrameworkToProject(target, "EventKitUI.framework", false);
        // proj.AddFrameworkToProject(target, "MediaPlayer.framework", false);
        //proj.AddFrameworkToProject (target, "MessageUI.framework", false);
        // proj.AddFrameworkToProject(target, "QuartzCore.framework", false);
        // proj.AddFrameworkToProject(target, "SystemConfiguration.framework", false);
        // proj.AddFrameworkToProject(target, "CoreFoundation.framework", false);

        // proj.AddFileToBuild(target, proj.AddFile("usr/lib/libz.1.2.5.dylib", "Frameworks/libz.1.2.5.dylib", PBXSourceTree.Sdk));

        //Optional Frameworks
        //proj.AddFrameworkToProject (target, "AdSupport.framework", true);
        // proj.AddFrameworkToProject(target, "Social.framework", true);
        //proj.AddFrameworkToProject (target, "StoreKit.framework", true);
        // proj.AddFrameworkToProject(target, "Webkit.framework", true);

        //proj.AddFrameworkToProject (target, "MobileCoreServices.framework", false);
        //proj.AddFrameworkToProject (target, "GLKit.framework", false);
        // proj.AddFrameworkToProject(target, "CoreMotion.framework", false);

        // //Analytics (optional for Google Analytics)
        // proj.AddFrameworkToProject(target, "CoreData.framework", true);
        // proj.AddFrameworkToProject(target, "libsqlite3.dylib", true);
        // proj.AddFrameworkToProject(target, "libxml2.tbd", false);

        // proj.AddFrameworkToProject(target, "AddressBook.framework", true);

        // proj.AddFrameworkToProject(target, "libz.tbd", true);

        proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
        // proj.SetBuildProperty(target, "CLANG_ENABLE_MODULES", "YES");

        File.WriteAllText(projPath, proj.WriteToString());
    }

    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {

        if (buildTarget == BuildTarget.iOS)
        {

            // Get plist
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            // Get root
            PlistElementDict rootDict = plist.root;

            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            // rootDict.CreateDict("NSCalendarsUsageDescription");
            // rootDict.SetString("NSCalendarsUsageDescription", "Adding events");

            // rootDict.CreateDict("NSPhotoLibraryUsageDescription");
            // rootDict.SetString("NSPhotoLibraryUsageDescription", "Advertisement use");

            // rootDict.CreateDict("NSMotionUsageDescription");
            // rootDict.SetString("NSMotionUsageDescription", "Advertisement use");

            // rootDict.CreateDict("NSLocationWhenInUseUsageDescription");
            // rootDict.SetString("NSLocationWhenInUseUsageDescription", "Use Location");

            // rootDict.CreateDict("NSBluetoothPeripheralUsageDescription");
            // rootDict.SetString("NSBluetoothPeripheralUsageDescription", "Advertisement would like to use bluetooth.");

            // rootDict.CreateDict("NSContactsUsageDescription");
            // rootDict.SetString("NSContactsUsageDescription", "Use Contact");

            // rootDict.CreateDict("ITSAppUsesNonExemptEncryption");
            // rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            // PlistElementArray bundleURLTypes = rootDict.CreateArray("CFBundleURLTypes");

            // PlistElementDict bundleURLTypesDic = bundleURLTypes.AddDict();

            // bundleURLTypesDic.CreateDict("CFBundleURLName");
            // bundleURLTypesDic.SetString("CFBundleURLName", BUNDLE_ID);

            // PlistElementArray bundleURLSchemes = bundleURLTypesDic.CreateArray("CFBundleURLSchemes");
            // bundleURLSchemes.AddString(URL_SCEME);

            //            PlistElementArray queriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
            //            queriesSchemes.AddString("fb");
            //            queriesSchemes.AddString("instagram");
            //            queriesSchemes.AddString("tumblr");
            //            queriesSchemes.AddString("twitter");
            //            queriesSchemes.AddString("tel");
            //            queriesSchemes.AddString("sms");

            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
#endif


}