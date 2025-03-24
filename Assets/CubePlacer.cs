using System.Collections;
using UnityEngine;

using UnityEngine.InputSystem;

public class CubePlacer : MonoBehaviour
{
    [Header("References")]
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor;
    public GameObject cubePrefab;
    public GameObject placementIndicator;
    public InputActionReference placeAction;

    [Header("Settings")]
    public float maxPlacementDistance = 10f;
    public float placementCooldown = 0.2f;

    private Vector3 placementPosition;
    private bool canPlace = true;
    private bool isCurrentlyHitting = false;

    void OnEnable()
    {
        if (placeAction != null)
            placeAction.action.performed += OnPlaceActionPerformed;
    }

    void OnDisable()
    {
        if (placeAction != null)
            placeAction.action.performed -= OnPlaceActionPerformed;
    }

    void Update()
    {
        // Check if ray hits something using the current system
        if (rayInteractor.TryGetHitInfo(out Vector3 position, out Vector3 normal, out int positionInLine, out bool isValidTarget))
        {
            // Calculate distance
            float distance = Vector3.Distance(rayInteractor.transform.position, position);

            if (distance <= maxPlacementDistance && isValidTarget)
            {
                // Calculate the position adjacent to the hit face
                Vector3 placePos = position + normal * 0.5f;

                // Round to grid (for Minecraft-like placement)
                placementPosition = new Vector3(
                    Mathf.Round(placePos.x),
                    Mathf.Round(placePos.y),
                    Mathf.Round(placePos.z)
                );

                // Show placement indicator
                placementIndicator.SetActive(true);
                placementIndicator.transform.position = placementPosition;
                isCurrentlyHitting = true;
            }
            else
            {
                HideIndicator();
            }
        }
        else
        {
            HideIndicator();
        }
    }

    private void OnPlaceActionPerformed(InputAction.CallbackContext context)
    {
        if (canPlace && isCurrentlyHitting)
        {
            PlaceCube();
        }
    }

    private void HideIndicator()
    {
        placementIndicator.SetActive(false);
        isCurrentlyHitting = false;
    }

    private void PlaceCube()
    {
        // Instantiate cube at placement position
        Instantiate(cubePrefab, placementPosition+new Vector3(0,1,0), Quaternion.identity);

        // Add cooldown to prevent multiple placements
        StartCoroutine(PlacementCooldown());
    }

    private IEnumerator PlacementCooldown()
    {
        canPlace = false;
        yield return new WaitForSeconds(placementCooldown);
        canPlace = true;
    }
}