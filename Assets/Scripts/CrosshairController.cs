using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class CrosshairController : MonoBehaviour
{
    public float range;
    public float idleSpinVelocity;
    public float firingSpinVelocity;
	public GameObject pointLight;

	private Quaternion spin;
	private InputAction fireAction;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
		spin = Quaternion.identity;
		var color = transform.parent.GetComponent<PlayerController>().primaryColor;
        GetComponent<SpriteRenderer>().color = color;
        pointLight.GetComponent<Light2D>().color = color;

        fireAction = InputSystem.actions.FindAction("Attack");
    }

    // Update is called once per frame
    void Update()
    {
        // Update position
        Vector3 mousePos = Mouse.current.position.ReadValue();
        Vector3 parentPos = Camera.main.WorldToScreenPoint(transform.parent.gameObject.transform.position);
        parentPos.z = 0.0f;
		Vector3 aimDirection = Vector3.Normalize(mousePos - parentPos);
		Vector3 scaledAimDirection = aimDirection * range;

		transform.localPosition = scaledAimDirection;

		// Update spin
        var spinVelocity = fireAction.IsPressed() ? firingSpinVelocity : idleSpinVelocity;
        spin = Quaternion.Euler(0, 0, spinVelocity) * spin;
        var totalRotation = spin;

		// Add base rotation
		totalRotation = Quaternion.Euler(0, 70, 0) * totalRotation;

		// Match rotation to aim vector
		float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
		var matchQuaternion = Quaternion.AngleAxis(angle, new Vector3(0,0,1));
		totalRotation = matchQuaternion * totalRotation;
		transform.localRotation = totalRotation;

        pointLight.transform.rotation = Quaternion.identity;
    }
}
