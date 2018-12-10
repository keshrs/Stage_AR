namespace StageAR
{
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = GoogleARCore.InstantPreviewInput;
#endif

    // Main Controller for the spatial audio project
    public class MainController : MonoBehaviour
    {
        // This is the camera which renders the AR background, aka real world.
        public Camera FirstPersonCamera;

        // A prefab for tracking and visualizing detected planes.
        // Currently default ARCore object.
        public GameObject DetectedPlanePrefab;

        // A model to place when a raycast from a user touch hits a plane.
        // Currently a palm tree -- can be modified in Unity
        public GameObject ModelPrefab_1;
        
        // An alternative model to place when a raycast from a user touch hits a plane.
        // Currently a poplar tree -- can be modified in Unity
        public GameObject ModelPrefab_2;

        // A toggle variable to change which model is spawned on touch.
        public bool ModelPrefabToggle = true;

        public void ToggleModelPrefab()
        {
            ModelPrefabToggle = !ModelPrefabToggle;
        }
        
        // A gameobject parent of UI for displaying the "searching for planes" snackbar.
        public GameObject SearchingForPlaneUI;
        
        // The rotation in degrees to apply to model when placed.
        private const float k_ModelRotation = 180.0f;
        
        // A list to hold all planes ARCore is tracking in the current frame. This object is used across
        // the application to avoid per-frame allocations.
        private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();
        
        /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
        private bool m_IsQuitting = false;


        public void Update()
        {
            _UpdateApplicationLifecycle();

            // Hide snackbar when tracking at least one plane.
            Session.GetTrackables<DetectedPlane>(m_AllPlanes);
            bool showSearchingUI = true;
            for (int i = 0; i < m_AllPlanes.Count; i++)
            {
                if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
                {
                    showSearchingUI = false;
                    break;
                }
            }

            SearchingForPlaneUI.SetActive(showSearchingUI);

            // If the player has touched the screen, spawn the selected model.
            Touch touch = Input.GetTouch(0);
            if (Input.touchCount >= 1 || (touch.phase == TouchPhase.Began))
            {
                SpawnModel(touch);
            }
        }

        /// Spawns the selected model at location of raycasted touch
        private void SpawnModel(Touch touch)
        {
            /// Model collision handling here
            Ray raycast = FirstPersonCamera.ScreenPointToRay(touch.position);
            RaycastHit raycastHit;
            if (Physics.Raycast(raycast, out raycastHit))
            {
                // All (currently two) virtual objects have been tagged accordingly in order to be detected
                string tag = raycastHit.collider.gameObject.tag;
                if (tag.Equals("VirtualObject"))
                {
                    // This assigment selects what prefab the siblings are created from
                    GameObject prefab_1 = ModelPrefabToggle ? ModelPrefab_1 : ModelPrefab_2;

                    // This is the object sibling positions will be relative to
                    var baseObject = raycastHit.collider.gameObject;

                    // Set offset for left and right siblings to be random, with min distance
                    Vector3 vOffsetLeft = baseObject.transform.position;
                        vOffsetLeft.x -= (float)0.2 + (Random.value * (float)0.8);
                        vOffsetLeft.z += (float)0.2 + (Random.value * (float)0.8);
                    Vector3 vOffsetRight = baseObject.transform.position;
                        vOffsetRight.x += (float)0.2 + (Random.value * (float)0.8);
                        vOffsetRight.z += (float)0.2 + (Random.value * (float)0.8);

                    // Instantiate sibling models at offset the hit pose.
                    var siblingLeft = Instantiate(prefab_1, vOffsetLeft, baseObject.transform.rotation);
                    var siblingRight = Instantiate(prefab_1, vOffsetRight, baseObject.transform.rotation);

                    // Each sibling transform is parented by the original object
                    siblingLeft.transform.parent = baseObject.transform;
                    siblingRight.transform.parent = baseObject.transform;

                    // Bypass plane collisions detection if we hit an object
                    return;
                }
            }

            /// Plane collion handling below

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;
            
            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hit test is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    // Choose the Model model for the Trackable that got hit.
                    GameObject prefab;
                    if (hit.Trackable is FeaturePoint)
                    {
                        prefab = ModelPrefabToggle ? ModelPrefab_2 : ModelPrefab_1;
                    }
                    else
                    {
                        prefab = ModelPrefabToggle ? ModelPrefab_1 : ModelPrefab_2;
                    }

                    // Instantiate Model model at the hit pose.
                    var modelObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                    // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                    modelObject.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                    // world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    // Make the model a child of the anchor.
                    modelObject.transform.parent = anchor.transform;
                }
            }
        }

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                const int lostTrackingSleepTimeout = 15;
                Screen.sleepTimeout = lostTrackingSleepTimeout;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void _DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
