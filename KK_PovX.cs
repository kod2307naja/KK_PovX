﻿using BepInEx;
using BepInEx.Configuration;
using BepInEx.Harmony;
using UnityEngine;

namespace KK_PovX
{
	[BepInProcess("Koikatu")]
	[BepInProcess("Koikatsu Party")]
	[BepInPlugin(GUID, PluginName, Version)]
	public partial class KK_PovX : BaseUnityPlugin
	{
		public const string GUID = "com.2155x.fairbair.kk_povx";
		public const string PluginName = "KK_PovX";
		public const string Version = "1.0.0";


		const string SECTION_GENERAL = "General";
		const string SECTION_CAMERA = "Camera";
		const string SECTION_ANIMATION = "Animation";
		const string SECTION_HOTKEYS = "Hotkeys";

		const string DESCRIPTION_OFFSET_X =
			"Sideway offset from the character's eyes.";
		const string DESCRIPTION_OFFSET_Y =
			"Vertical offset from the character's eyes.";
		const string DESCRIPTION_OFFSET_Z =
			"Forward offset from the character's eyes.";
		const string DESCRIPTION_CAMERA_MIN_X =
			"Highest downward and leftward angle the camera can rotate.";
		const string DESCRIPTION_CAMERA_MAX_X =
			"Highest upward and rightware angle the camera can rotate.";
		const string DESCRIPTION_CAMERA_SPAN_Y =
			"How far can the camera be rotated horizontally? " +
			"Only applies during scenes where the player character can't move.";
		const string DESCRIPTION_CAMERA_STABILIZE =
			"When enabled, reduces camera shake by getting the median position. " +
			"Only applies during H scene.";
		const string DESCRIPTION_CAMERA_HEAD_ROTATE =
			"When enabled, the head rotates along with the camera. " +
			"This may ruin some animations when selecting the girls, " +
			"such as blowjobs. " +
			"Does not apply to free roam mode.";

		const string DESCRIPTION_ROTATE_HEAD_FIRST =
			"Should the head rotate first before turning the whole body? " +
			"Only applies with free roam mode.";
		const string DESCRIPTION_NECK_MIN =
			"Highest downward angle the head can rotate.";
		const string DESCRIPTION_NECK_MAX =
			"Highest upward angle the head can rotate.";
		const string DESCRIPTION_HEAD_MAX =
			"The farthest the head can rotate until the body would rotate. " +
			"Only applies during free roam mode.";

		const string DESCRIPTION_CHARA_CYCLE_KEY =
			"Switch between characters during PoV mode. " +
			"Only applies during H scene.";
		const string DESCRIPTION_CAMERA_DRAG_KEY =
			"During PoV mode, holding down this key will move the camera if the mouse isn't locked.";
		const string DESCRIPTION_TOGGLE_CURSOR_KEY =
			"Pressing this key will toggle the cursor during PoV mode in H scenes. " +
			"Pressing any other keys will reveal the cursor.";
		const string DESCRIPTION_HIDE_HEAD =
			"Should the head be invisible when in PoV mode? " +
			"Head is always invisible during H scenes or " +
			"situations where the player can't move.";

		public static ConfigEntry<bool> HideHead { get; set; }

		public static ConfigEntry<float> Sensitivity { get; set; }
		public static ConfigEntry<float> Fov { get; set; }
		public static ConfigEntry<float> ZoomFov { get; set; }
		public static ConfigEntry<float> OffsetX { get; set; }
		public static ConfigEntry<float> OffsetY { get; set; }
		public static ConfigEntry<float> OffsetZ { get; set; }
		public static ConfigEntry<float> CameraMinX { get; set; }
		public static ConfigEntry<float> CameraMaxX { get; set; }
		public static ConfigEntry<float> CameraSpanY { get; set; }
		public static ConfigEntry<bool> CameraStabilize { get; set; }
		public static ConfigEntry<bool> CameraHeadRotate { get; set; }

		public static ConfigEntry<bool> RotateHeadFirst { get; set; }
		public static ConfigEntry<float> NeckMin { get; set; }
		public static ConfigEntry<float> NeckMax { get; set; }
		public static ConfigEntry<float> HeadMax { get; set; }

		public static ConfigEntry<KeyboardShortcut> PovKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> CharaCycleKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> CameraDragKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> ToggleCursorKey { get; set; }
		public static ConfigEntry<KeyboardShortcut> ZoomKey { get; set; }

		public void Awake()
		{
			HideHead = Config.Bind(SECTION_GENERAL, "Hide Head", true, DESCRIPTION_HIDE_HEAD);

			Sensitivity = Config.Bind(SECTION_CAMERA, "Camera Sensitivity", 2f);
			Fov = Config.Bind(SECTION_CAMERA, "Field of View", 50f);
			ZoomFov = Config.Bind(SECTION_CAMERA, "Zoom Field of View", 5f);
			OffsetX = Config.Bind(SECTION_CAMERA, "Offset X", 0f, DESCRIPTION_OFFSET_X);
			OffsetY = Config.Bind(SECTION_CAMERA, "Offset Y", 0f, DESCRIPTION_OFFSET_Y);
			OffsetZ = Config.Bind(SECTION_CAMERA, "Offset Z", 0.03f, DESCRIPTION_OFFSET_Z);
			CameraMinX = Config.Bind(SECTION_CAMERA, "Min Camera Angle X", 80f, DESCRIPTION_CAMERA_MIN_X);
			CameraMaxX = Config.Bind(SECTION_CAMERA, "Max Camera Angle X", 80f, DESCRIPTION_CAMERA_MAX_X);
			CameraSpanY = Config.Bind(SECTION_CAMERA, "Camera Angle Span Y", 70f, DESCRIPTION_CAMERA_SPAN_Y);
			CameraStabilize = Config.Bind(SECTION_CAMERA, "Stabilize Camera", false, DESCRIPTION_CAMERA_STABILIZE);
			CameraHeadRotate = Config.Bind(SECTION_CAMERA, "Rotate Head to Camera", false, DESCRIPTION_CAMERA_HEAD_ROTATE);

			RotateHeadFirst = Config.Bind(SECTION_ANIMATION, "Rotate Head First", true, DESCRIPTION_ROTATE_HEAD_FIRST);
			NeckMin = Config.Bind(SECTION_ANIMATION, "Min Neck Angle X", 0f, DESCRIPTION_NECK_MIN);
			NeckMax = Config.Bind(SECTION_ANIMATION, "Max Neck Angle X", 90f, DESCRIPTION_NECK_MAX);
			HeadMax = Config.Bind(SECTION_ANIMATION, "Max Head Angle Y", 60f, DESCRIPTION_HEAD_MAX);

			PovKey = Config.Bind(SECTION_HOTKEYS, "PoV Toggle Key", new KeyboardShortcut(KeyCode.Comma));
			CharaCycleKey = Config.Bind(SECTION_HOTKEYS, "Character Cycle Key", new KeyboardShortcut(KeyCode.Period), DESCRIPTION_CHARA_CYCLE_KEY);
			CameraDragKey = Config.Bind(SECTION_HOTKEYS, "Camera Drag Key", new KeyboardShortcut(KeyCode.Mouse0), DESCRIPTION_CAMERA_DRAG_KEY);
			ToggleCursorKey = Config.Bind(SECTION_HOTKEYS, "Toggle Cursor Key", new KeyboardShortcut(KeyCode.LeftControl), DESCRIPTION_TOGGLE_CURSOR_KEY);
			ZoomKey = Config.Bind(SECTION_HOTKEYS, "Zoom Key", new KeyboardShortcut(KeyCode.X));

			HideHead.SettingChanged += (sender, args) =>
			{
				Controller.SetChaControl(Controller.FromFocus());
			};

			HarmonyWrapper.PatchAll(typeof(KK_PovX));
		}

		public void Update()
		{
			if (Tools.IsInGame())
				Controller.Update();
			else if (Controller.toggled)
				Controller.TogglePoV(false);
		}
	}
}