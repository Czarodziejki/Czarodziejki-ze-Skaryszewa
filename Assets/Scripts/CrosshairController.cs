using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class CrosshairController : MonoBehaviour
{
    private Quaternion spin;
    public float range;
    public float idleSpinVelocity;
    public float firingSpinVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		spin = Quaternion.identity;
	}

    // Update is called once per frame
    void Update()
    {
        // Update position
        Vector3 mousePos = Input.mousePosition;
        Vector3 parentPos = Camera.main.WorldToScreenPoint(transform.parent.gameObject.transform.position);
		Vector3 aimDirection = Vector3.Normalize(mousePos - parentPos);
		Vector3 scaledAimDirection = aimDirection * range;

		transform.localPosition = scaledAimDirection;

        var totalRotation = Quaternion.identity;
		// Update spin
        var spinVelocity = Input.GetKey(KeyCode.Mouse0) ? firingSpinVelocity : idleSpinVelocity;
        spin = Quaternion.Euler(0, 0, spinVelocity) * spin;
        totalRotation = spin * totalRotation;

		// Add base rotation
		totalRotation = Quaternion.Euler(0, 70, 0) * totalRotation;

		// Match rotation to aim vector
		float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
		var matchQuaternion = Quaternion.AngleAxis(angle, new Vector3(0,0,1));
		totalRotation = matchQuaternion * totalRotation;
		transform.localRotation = totalRotation;
    }
}
