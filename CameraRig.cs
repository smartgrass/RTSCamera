using UnityEngine;

namespace Core.Camera
{

	public class CameraRig : MonoBehaviour
	{
        //调整高度用
        public float zoomDist;//{ get; private set; }

		/// Camera angle when fully zoomed in
		public Transform zoomedCamAngle;

		/// Map size, edited through the CameraRigEditor script in edit mode
		//[HideInInspector]
		public Rect mapSize = new Rect(-10, -10, 20, 20);

        // Current reusable floor plane
        Plane m_FloorPlane;
		public Plane floorPlane
		{
			get { return m_FloorPlane; }
		}
        #region 临时变量
        // Look dampening factor
        float lookDampFactor;
        // Movement dampening factor
        float movementDampFactor;
        // Current look velocity of camera
        Vector3 m_CurrentLookVelocity;
        // Rotations of camera at various zoom levels

        Quaternion m_MinZoomRotation;
        Quaternion m_MaxZoomRotation;

        //Current camera velocity
        Vector3 m_CurrentCamVelocity; 
        #endregion


	/// <summary>
		/// Target position on the grid that we're looking at
		/// </summary>
		public Vector3 lookPosition { get; private set; }

		/// <summary>
		/// Current look position of camera
		/// </summary>
		public Vector3 currentLookPosition { get; private set; }

		/// <summary>
		/// Target position of the camera
		/// </summary>
		public Vector3 cameraPosition { get; private set; }
/// <summary>
		/// Bounds of our look area, related to map size, zoom level and aspect ratio/screen size
		/// </summary>
		public Rect lookBounds { get; private set; }


		/// <summary>
		/// Gets the unit we're tracking if any
		/// </summary>
		public GameObject trackingObject { get; private set; }

		/// <summary>
		/// Cached camera component
		/// </summary>
		public UnityEngine.Camera cachedCamera { get; private set; }

		/// <summary>
		/// Initialize references and floor plane
		/// </summary>
		protected virtual void Awake()
		{
			cachedCamera = GetComponent<UnityEngine.Camera>();
			m_FloorPlane = new Plane(Vector3.up, new Vector3(0.0f, 0, 0.0f));
			
			// Set initial values
			var lookRay = new Ray(cachedCamera.transform.position, cachedCamera.transform.forward);

			float dist;
			if (m_FloorPlane.Raycast(lookRay, out dist))
			{
				currentLookPosition = lookPosition = lookRay.GetPoint(dist);
			}
			cameraPosition = cachedCamera.transform.position;

			m_MinZoomRotation = Quaternion.FromToRotation(Vector3.up, -cachedCamera.transform.forward);
			m_MaxZoomRotation = Quaternion.FromToRotation(Vector3.up, -zoomedCamAngle.transform.forward);
			zoomDist = (currentLookPosition - cameraPosition).magnitude;
		}

		/// <summary>
		/// Setup initial zoom level and camera bounds
		/// </summary>
		protected virtual void Start()
		{
			RecalculateBoundingRect();
		}

		/// <summary>
		/// Handle camera behaviour
		/// </summary>
		protected virtual void Update()
		{
			RecalculateBoundingRect();

			// Tracking?
			if (trackingObject != null)
			{
				PanTo(trackingObject.transform.position);

				if (!trackingObject.activeInHierarchy)
				{
					StopTracking();
				}
			}

			// Approach look position
			currentLookPosition = Vector3.SmoothDamp(currentLookPosition, lookPosition, ref m_CurrentLookVelocity,lookDampFactor);

			Vector3 worldPos = transform.position;
			worldPos = Vector3.SmoothDamp(worldPos, cameraPosition, ref m_CurrentCamVelocity,movementDampFactor);

			transform.position = worldPos;
			transform.LookAt(currentLookPosition);
		}

#if UNITY_EDITOR
		/// <summary>
		/// Debug bounds area gizmo
		/// </summary>
		void OnDrawGizmosSelected()
		{
			// We dont want to display this in edit mode
			if (!Application.isPlaying)
			{
				return;
			}
			if (cachedCamera == null)
			{
				cachedCamera = GetComponent<UnityEngine.Camera>();
			}
			RecalculateBoundingRect();
			Gizmos.color = Color.red;

			Gizmos.DrawLine(
				new Vector3(lookBounds.xMin, 0.0f, lookBounds.yMin),
				new Vector3(lookBounds.xMax, 0.0f, lookBounds.yMin));
			Gizmos.DrawLine(
				new Vector3(lookBounds.xMin, 0.0f, lookBounds.yMin),
				new Vector3(lookBounds.xMin, 0.0f, lookBounds.yMax));
			Gizmos.DrawLine(
				new Vector3(lookBounds.xMax, 0.0f, lookBounds.yMax),
				new Vector3(lookBounds.xMin, 0.0f, lookBounds.yMax));
			Gizmos.DrawLine(
				new Vector3(lookBounds.xMax, 0.0f, lookBounds.yMax),
				new Vector3(lookBounds.xMax, 0.0f, lookBounds.yMin));

			Gizmos.color = Color.yellow;

			Gizmos.DrawLine(transform.position, currentLookPosition);
		}
#endif

		/// <summary>
		/// Pans the camera to a specific position
		/// </summary>
		/// <param name="position">The look target</param>
		public void PanTo(Vector3 position)
		{
			Vector3 pos = position;

            // Look position is floor height
            pos.y = 0;

			// Clamp to look bounds
			pos.x = Mathf.Clamp(pos.x, lookBounds.xMin, lookBounds.xMax);
			pos.z = Mathf.Clamp(pos.z, lookBounds.yMin, lookBounds.yMax);
			lookPosition = pos;

			// Camera position calculated from look position with view vector and zoom dist
			cameraPosition = lookPosition + (GetToCamVector() * zoomDist);
		}

		/// <summary>
		/// Cause the camera to follow a unit
		/// </summary>
		/// <param name="objectToTrack"></param>
		public void TrackObject(GameObject objectToTrack)
		{
			trackingObject = objectToTrack;
			PanTo(trackingObject.transform.position);
		}

		/// <summary>
		/// Stop tracking a unit
		/// </summary>
		public void StopTracking()
		{
			trackingObject = null;
		}

		/// <summary>
		/// Pan the camera
		/// </summary>
		/// <param name="panDelta">How far to pan the camera, in world space units</param>
		public void PanCamera(Vector3 panDelta)
		{
			Vector3 pos = lookPosition;
			pos += panDelta;

			// Clamp to look bounds
			pos.x = Mathf.Clamp(pos.x, lookBounds.xMin, lookBounds.xMax);
			pos.z = Mathf.Clamp(pos.z, lookBounds.yMin, lookBounds.yMax);
			lookPosition = pos;

			// Camera position calculated from look position with view vector and zoom dist
			cameraPosition = lookPosition + (GetToCamVector() * zoomDist);
		}


		/// <summary>
		/// Gets the screen position of a given world position
		/// </summary>
		/// <param name="worldPos">The world position</param>
		/// <returns>The screen position of that point</returns>
		public Vector3 GetScreenPos(Vector3 worldPos)
		{
			return cachedCamera.WorldToScreenPoint(worldPos);
		}

		/// <summary>
		/// Gets the to camera vector based on our current zoom level
		/// </summary>
		Vector3 GetToCamVector()
		{
			/*float t = Mathf.Clamp01((zoomDist - nearestZoom) / (furthestZoom - nearestZoom));
			t = 1 - ((1 - t) * (1 - t));*/
			Quaternion interpolatedRotation = Quaternion.Slerp(
				m_MaxZoomRotation, m_MinZoomRotation,
				0.1f);
			return interpolatedRotation * Vector3.up;
		}

		/// <summary>
		/// Update the size of our camera's bounding rectangle
		/// </summary>
		void RecalculateBoundingRect()
		{
			Rect mapsize = mapSize;

			// Get some world space projections at this zoom level
			// Temporarily move camera to final look position
			Vector3 prevCameraPos = transform.position;
			transform.position = cameraPosition;
			transform.LookAt(lookPosition);

			// Project screen corners and center
			var bottomLeftScreen = new Vector3(0, 0);
			var topLeftScreen = new Vector3(0, Screen.height);
			var centerScreen = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f);

			Vector3 bottomLeftWorld = Vector3.zero;
			Vector3 topLeftWorld = Vector3.zero;
			Vector3 centerWorld = Vector3.zero;
			float dist;

			Ray ray = cachedCamera.ScreenPointToRay(bottomLeftScreen);
			if (m_FloorPlane.Raycast(ray, out dist))
			{
				bottomLeftWorld = ray.GetPoint(dist);
			}

			ray = cachedCamera.ScreenPointToRay(topLeftScreen);
			if (m_FloorPlane.Raycast(ray, out dist))
			{
				topLeftWorld = ray.GetPoint(dist);
			}

			ray = cachedCamera.ScreenPointToRay(centerScreen);
			if (m_FloorPlane.Raycast(ray, out dist))
			{
				centerWorld = ray.GetPoint(dist);
			}

			Vector3 toTopLeft = topLeftWorld - centerWorld;
			Vector3 toBottomLeft = bottomLeftWorld - centerWorld;

			lookBounds = new Rect(
				mapsize.xMin - toBottomLeft.x,
				mapsize.yMin - toBottomLeft.z,
				Mathf.Max(mapsize.width + (toBottomLeft.x * 2), 0),
				Mathf.Max((mapsize.height - toTopLeft.z) + toBottomLeft.z, 0));

			// Restore camera position
			transform.position = prevCameraPos;
			transform.LookAt(currentLookPosition);
		}
	}
}