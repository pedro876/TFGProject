using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FunctionViewController : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] GameObject pressSpaceObj;

    private IPostProcessFacade postProcessFacade;
    private RawImage image;
    private bool blocked = false;

    IViewFacade viewFacade;

    void Start()
    {
        image = GetComponent<RawImage>();
        postProcessFacade = ServiceLocator.Instance.GetService<IPostProcessFacade>();
        viewFacade = ServiceLocator.Instance.GetService<IViewFacade>();
        image.texture = postProcessFacade.DisplayTexture;
        postProcessFacade.onDisplayUpdated += () => image.texture = postProcessFacade.DisplayTexture;
        Release();
    }

    private void Update()
    {
        if (blocked)
        {
            if(Cursor.lockState != CursorLockMode.Locked || Input.GetKeyDown(KeyCode.Space))
            {
                Release();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Block();
    }

    private void Block()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        blocked = true;
        pressSpaceObj.SetActive(true);
        viewFacade.SetCanMove(true);
    }

    private void Release()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        blocked = false;
        pressSpaceObj.SetActive(false);
        viewFacade.SetCanMove(false);
    }
}
