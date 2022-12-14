////////////////////////////////////////////////////////////////////////////////
//  
// @author Benoît Freslon @benoitfreslon
// https://github.com/BenoitFreslon/Vibration
// https://benoitfreslon.com
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class Vibration {

#if UNITY_IOS && !UNITY_EDITOR

    [DllImport ( "__Internal" )]
    static extern bool _HasVibrator ();

    [DllImport ( "__Internal" )]
    static extern void _Vibrate ();

    [DllImport ( "__Internal" )]
    static extern void _VibratePop ();

    [DllImport ( "__Internal" )]
    static extern void _VibratePeek ();

    [DllImport ( "__Internal" )]
    static extern void _VibrateNope ();
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass ( "com.unity3d.player.UnityPlayer" );
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject> ( "currentActivity" );
    public static AndroidJavaObject vibrator =
 currentActivity.Call<AndroidJavaObject> ( "getSystemService", "vibrator" );
    public static AndroidJavaObject context = currentActivity.Call<AndroidJavaObject> ( "getApplicationContext" );
#endif

    ///<summary>
    ///Only on iOS
    ///</summary>
    public static void VibratePop() {
        if (!IsOnMobile()) 
            return;

#if UNITY_IOS && !UNITY_EDITOR
        _VibratePop ();
#elif UNITY_ANDROID && !UNITY_EDITOR
        Vibrate (100);
#endif
    }

    ///<summary>
    ///Only on iOS
    ///</summary>
    public static void VibratePeek() {
        if (!IsOnMobile()) 
            return;

#if UNITY_IOS && !UNITY_EDITOR
        _VibratePeek ();
#elif UNITY_ANDROID && !UNITY_EDITOR
        Vibrate (8);
#endif
    }

    ///<summary>
    ///Only on iOS
    ///</summary>
    public static void VibrateNope() {
        if (!IsOnMobile()) 
            return;

#if UNITY_IOS && !UNITY_EDITOR
        _VibrateNope ();
#elif UNITY_ANDROID && !UNITY_EDITOR
        Vibrate ();
#endif
    }

    public static void Vibrate() {
        Handheld.Vibrate();
    }

    ///<summary>
    /// Only on Android
    /// https://developer.android.com/reference/android/os/Vibrator.html#vibrate(long)
    ///</summary>
    public static void Vibrate(long milliseconds) {
        if (!IsOnMobile()) 
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        vibrator.Call ( "vibrate", milliseconds );
#elif UNITY_IOS && !UNITY_EDITOR
        _Vibrate ();
#endif
    }

    ///<summary>
    /// Only on Android
    /// https://proandroiddev.com/using-vibrate-in-android-b0e3ef5d5e07
    ///</summary>
    public static void Vibrate(long[] pattern, int repeat) {
        if (!IsOnMobile()) 
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        vibrator.Call ( "vibrate", pattern, repeat );
#elif UNITY_IOS && !UNITY_EDITOR
        _Vibrate ();
#endif
    }

    public static bool HasVibrator() {
        if (!IsOnMobile()) 
            return false;

#pragma warning disable 0162 // Unreachable code detected

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass contextClass = new AndroidJavaClass ( "android.content.Context" );
        string Context_VIBRATOR_SERVICE = contextClass.GetStatic<string> ( "VIBRATOR_SERVICE" );
        AndroidJavaObject systemService =
 context.Call<AndroidJavaObject> ( "getSystemService", Context_VIBRATOR_SERVICE );
        if ( systemService.Call<bool> ( "hasVibrator" ) ) {
            return true;
        } else
            return false;
#elif UNITY_IOS && !UNITY_EDITOR
        return _HasVibrator ();
#endif
#pragma warning restore 0162 // Unreachable code detected
        return false;
    }

    ///<summary>
    ///Only on Android
    ///</summary>
    public static void Cancel() {
        if (!IsOnMobile()) 
            return;
#if UNITY_ANDROID && !UNITY_EDITOR
        vibrator.Call ( "cancel" );
#endif
    }

    static bool IsOnMobile() {
#if UNITY_EDITOR
        return false;
#endif
#pragma warning disable 0162 // Unreachable code detected
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            return true;

        return false;
#pragma warning restore 0162 // Unreachable code detected
    }
}
