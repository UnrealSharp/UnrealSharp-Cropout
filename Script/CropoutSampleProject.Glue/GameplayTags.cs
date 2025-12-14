using UnrealSharp.GameplayTags;

public static class GameplayTags
{
	public static readonly FGameplayTag Audio = new("Audio");
	public static readonly FGameplayTag Audio_AnimNotify = new("Audio.AnimNotify");
	public static readonly FGameplayTag Audio_AnimNotify_Footstep = new("Audio.AnimNotify.Footstep");
	public static readonly FGameplayTag EnhancedInput = new("EnhancedInput");
	public static readonly FGameplayTag EnhancedInput_Modes = new("EnhancedInput.Modes");
	public static readonly FGameplayTag EnhancedInput_Modes_Default = new("EnhancedInput.Modes.Default");
	public static readonly FGameplayTag Input = new("Input");
	public static readonly FGameplayTag Input_Gamepad = new("Input.Gamepad");
	public static readonly FGameplayTag Input_MouseAndKeyboard = new("Input.MouseAndKeyboard");
	public static readonly FGameplayTag Input_Touch = new("Input.Touch");
	public static readonly FGameplayTag InputMode = new("InputMode");
	public static readonly FGameplayTag InputMode_Game = new("InputMode.Game");
	public static readonly FGameplayTag InputMode_Menu = new("InputMode.Menu");
	public static readonly FGameplayTag InputUserSettings = new("InputUserSettings");
	public static readonly FGameplayTag InputUserSettings_FailureReasons = new("InputUserSettings.FailureReasons");
	public static readonly FGameplayTag InputUserSettings_FailureReasons_InvalidMappingName = new("InputUserSettings.FailureReasons.InvalidMappingName");
	public static readonly FGameplayTag InputUserSettings_FailureReasons_NoKeyProfile = new("InputUserSettings.FailureReasons.NoKeyProfile");
	public static readonly FGameplayTag InputUserSettings_FailureReasons_NoMappingRowFound = new("InputUserSettings.FailureReasons.NoMappingRowFound");
	public static readonly FGameplayTag InputUserSettings_FailureReasons_NoMatchingMappings = new("InputUserSettings.FailureReasons.NoMatchingMappings");
	public static readonly FGameplayTag InputUserSettings_Profiles = new("InputUserSettings.Profiles");
	public static readonly FGameplayTag InputUserSettings_Profiles_Default = new("InputUserSettings.Profiles.Default");
	public static readonly FGameplayTag Platform = new("Platform");
	public static readonly FGameplayTag Platform_Trait = new("Platform.Trait");
	public static readonly FGameplayTag Platform_Trait_PlayInEditor = new("Platform.Trait.PlayInEditor");
	public static readonly FGameplayTag SubBehaviour = new("SubBehaviour");
}