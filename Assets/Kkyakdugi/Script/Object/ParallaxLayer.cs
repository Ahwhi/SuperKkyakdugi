using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float _parallaxX = 0.5f;
    [SerializeField, Range(0f, 1f)] private float _parallaxY = 0f;

    private Transform   _cam;
    private Vector3     _lastCamPos;
    private Transform[] _tiles;
    private float       _tileWidth;

    private void Start()
    {
        _cam        = Camera.main.transform;
        _lastCamPos = _cam.position;

        int childCount = transform.childCount;
        if (childCount > 0)
        {
            _tiles     = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
                _tiles[i] = transform.GetChild(i);
            _tileWidth = _tiles[0].GetComponent<SpriteRenderer>().bounds.size.x;
        }
        else
        {
            _tiles     = new[] { transform };
            _tileWidth = GetComponent<SpriteRenderer>().bounds.size.x;
        }
    }

    private void LateUpdate()
    {
        Vector3 delta = _cam.position - _lastCamPos;
        transform.position += new Vector3(delta.x * _parallaxX, delta.y * _parallaxY, 0f);
        _lastCamPos = _cam.position;

        float loopWidth = _tileWidth * _tiles.Length;
        foreach (var tile in _tiles)
        {
            float camRelX = _cam.position.x - tile.position.x;
            if (camRelX > _tileWidth)
                tile.position += new Vector3(loopWidth, 0f, 0f);
            else if (camRelX < -_tileWidth)
                tile.position -= new Vector3(loopWidth, 0f, 0f);
        }
    }
}
