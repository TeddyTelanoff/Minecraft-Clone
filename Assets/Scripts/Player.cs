using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
	public Transform camTransform;
	public Camera firstPersonCamera;
	public Camera thirdPersonCamera;
	public float speed;
	public float maxSpeed;
	public float jumpForce;
	public float rotateSpeed;
	[Range(0, 1)]
	public float rotateSlerp;

	private Rigidbody rb;
	private CapsuleCollider ccollider;

	private Vector2 rot;
	private bool thirdPer;

	private Vector3Int bpic;
	private Vector2Int cp;
	private bool hb;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		ccollider = GetComponent<CapsuleCollider>();
	}

	private void FixedUpdate()
	{
		if (Input.GetButtonDown("Perspective"))
		{
			thirdPer = !thirdPer;
			firstPersonCamera.enabled = !thirdPer;
			thirdPersonCamera.enabled = thirdPer;
		}

		if (Input.GetButton("Jump") && OnGround())
			rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

		if (Input.GetKeyDown(KeyCode.LeftShift))
			Cursor.lockState = CursorLockMode.Locked;
		if (Input.GetKeyUp(KeyCode.LeftShift))
			Cursor.lockState = CursorLockMode.None;

		if (Input.GetKey(KeyCode.LeftShift))
		{
			rot.x -= Input.GetAxis("Mouse Y") * rotateSpeed;
			rot.y += Input.GetAxis("Mouse X") * rotateSpeed;

			rot.x = Mathf.Clamp(rot.x, -90, 90);

			transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(new Vector3 { y = rot.y }), rotateSlerp);
			camTransform.localRotation = Quaternion.Slerp(camTransform.localRotation, Quaternion.Euler(new Vector3 { x = rot.x }), rotateSlerp);
		}

		
		Ray ray = firstPersonCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.5f));
		if (hb = Physics.Raycast(ray, out RaycastHit hit, 4.5f, LayerMask.NameToLayer("Ground")))
		{
			Vector3 point = hit.point - hit.normal / 4 + Vector3.one / 2;
			Vector3Int blockPosInChunk = bpic = new Vector3Int(Mathf.FloorToInt(point.x % Chunk.SIZE.x),
				Mathf.FloorToInt(point.y % Chunk.SIZE.y),
				Mathf.FloorToInt(point.z % Chunk.SIZE.z));
			Chunk chunk = hit.transform.GetComponent<Chunk>();
			cp = ChunkGenerator.Instance.chunks.FirstOrDefault(c => c.Value == chunk).Key;

			if (Input.GetButtonDown("Destroy"))
			{
				chunk.blocks[blockPosInChunk.x, blockPosInChunk.y, blockPosInChunk.z].id = BlockID.AIR;
				chunk.GenerateMesh();
			}
			if (Input.GetButtonDown("Use"))
			{
				point = hit.point + Vector3.one / 2;
				blockPosInChunk = bpic = new Vector3Int(Mathf.FloorToInt(point.x % Chunk.SIZE.x),
					Mathf.FloorToInt(point.y % Chunk.SIZE.y),
					Mathf.FloorToInt(point.z % Chunk.SIZE.z));

				chunk.blocks[blockPosInChunk.x, blockPosInChunk.y, blockPosInChunk.z].id = BlockID.DIRT_BLOCK;
				chunk.GenerateMesh();
			}
		}
		else
			bpic = Vector3Int.zero;

		Vector3 force = Input.GetAxis("Z") * transform.forward + Input.GetAxis("X") * transform.right;
		if (force != Vector3.zero)
		{
			rb.AddForce(force * speed);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = new Color(255, 255, 255);
		if (hb)
			Gizmos.DrawWireCube(bpic + new Vector3(cp.x * Chunk.SIZE.x, 0, cp.y * Chunk.SIZE.z), Vector3.one);
	}

	public bool OnGround()
	{
		bool tl = Physics.Raycast(transform.position - ccollider.bounds.extents / 2, -Vector3.up, ccollider.bounds.extents.y + 0.01f, LayerMask.NameToLayer("Ground"));
		bool br = Physics.Raycast(transform.position + ccollider.bounds.extents / 2, -Vector3.up, ccollider.bounds.extents.y + 0.01f, LayerMask.NameToLayer("Ground"));
		bool tr = Physics.Raycast(transform.position - new Vector3((ccollider.bounds.extents / 2).x, 0), -Vector3.up, ccollider.bounds.extents.y + 0.01f, LayerMask.NameToLayer("Ground"));
		bool bl = Physics.Raycast(transform.position + new Vector3(0, (ccollider.bounds.extents / 2).y), -Vector3.up, ccollider.bounds.extents.y + 0.01f, LayerMask.NameToLayer("Ground"));
		bool middle = Physics.Raycast(transform.position, -Vector3.up, ccollider.bounds.extents.y + 0.01f, LayerMask.NameToLayer("Ground"));


		return tl || br || tr || bl || middle;
	}
}