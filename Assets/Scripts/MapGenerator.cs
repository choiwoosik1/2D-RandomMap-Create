using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    // �� ������ �ʿ��� private ������
    int[] _floorPlan;           // �� ��ü�� 1D �迭�� �����ϴ� ����

    int _floorPlanCount;        // ���ݱ��� ������ ��
    int _minRooms;              // �ּ� �� ����
    int _maxRooms;              // �ִ� �� ����
    List<int> _endRooms;        // ���� ������ ��� �� �� ���� �ε����� ��� ����Ʈ

    int _bossRoomIndex;         // ���� �� ���� �ε���
    int _secretRoomIndex;       // ��� �� ���� �ε���
    int _shopRoomIndex;         // ���� �� ���� �ε���
    int _itemRoomIndex;         // ������ �� ���� �ε���

    public Cell CellPrefab;     // ���� ȭ�鿡 ����� �� Prefab
    float _cellSize;            // �� ���� ����
    Queue<int> _cellQueue;      // �ܰ��� �� ���� �� ����� ť
    List<Cell> _spawnCells;     // Scene�� ������ Instantiate�� �� ������Ʈ

    [Header("---- ��������Ʈ ���� ----")]
    [SerializeField] Sprite _item;
    [SerializeField] Sprite _shop;
    [SerializeField] Sprite _boss;
    [SerializeField] Sprite _secret;

    void Start()
    {
        // �� ũ�� �� �� ������
        _minRooms = 7;
        _maxRooms = 15;
        _cellSize = 1f;

        // ���ο� ����Ʈ ����
        _spawnCells = new();

        SetUpDungeon();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetUpDungeon();
        }
    }

    /// <summary>
    /// ���� �� �ı� �� �� �ʱ�ȭ
    /// </summary>
    void SetUpDungeon() 
    {
        for(int i = 0; i < _spawnCells.Count; i++)
        {
            Destroy(_spawnCells[i].gameObject);
        }

        _spawnCells.Clear();

        _floorPlan = new int[100];
        _floorPlanCount = default;
        _cellQueue = new Queue<int>();
        _endRooms = new List<int>();

        // �߰� �� 45�� ����
        VisitCell(45);

        // ���� ���� �Լ� ȣ��
        GenerateDungeon();
    }

    void GenerateDungeon()
    {
        while (_cellQueue.Count > 0)   // ť�� �� ������ �ݺ�
        {
            // Dequeue : ť���� �����͸� ������ ����
            int index = _cellQueue.Dequeue();

            // index �� 1���� �迭 ���� ��ġ��
            // x �� ������ǥ (�� ��ȣ)
            int x = index % 10;

            bool created = false;

            // ���� �� Ȯ�� (x > 1�̸�, �� �� ���� ���� �ƴϸ�)
            // |= ��Ʈ OR ���� ������
            // A |= B -> A = A|B
            if (x > 1) created |= VisitCell(index - 1);

            // ������ �� Ȯ�� (x < 9�̸�, �� �� ������ ���� �ƴϸ�)
            if (x < 9) created |= VisitCell(index + 1);

            // ���� �� Ȯ�� (index > 20�̸�, ���� �ö󰡵� ���� ����)
            if (index > 20) created |= VisitCell(index - 10);

            // �Ʒ��� �� Ȯ�� (index < 70�̸�, �Ʒ��� �������� ���� ����)
            if (index < 70) created |= VisitCell(index + 10);

            // ���� �� ��Ͽ� �߰�
            if (created == false)
            {
                _endRooms.Add(index);
            }
        }

        if(_floorPlanCount < _minRooms)
        {
            SetUpDungeon();
            return;
        }

        SetUpSpecialRooms();
    }


    void SetUpSpecialRooms() { }

    void UpdateSpecialRoomVisuals() { }

    int RandomEndRoom()
    {
        return -1;
    }

    int PickSecretRoom()
    {
        return -1;
    }

    /// <summary>
    /// �迭 ����� �ε��� �˻� ��
    /// �ش� ����� ���� ���� Ȯ�� �� 0~4 ������ ���� ����
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private int GetNeighbourCount(int index)
    {
        // �� �� �� ���� �� ���� ���� �ִ� �� Ȯ��
        // ���� _floorPlan[index - 1]�� 1�̰� �������� 0�̸� �̿� �� ���� 1
        return _floorPlan[index - 10] + _floorPlan[index - 1] + _floorPlan[index + 1] + _floorPlan[index + 10];  
    }

    /// <summary>
    /// ���� �湮�ϴ� �Լ�
    /// �� ���� Ȯ�ο��� �����ϸ� ���� Cell ���� ���� false ��ȯ
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private bool VisitCell(int index)
    {
        // ��� �ִ� ���� �ƴ��� Ȯ��
        if (_floorPlan[index] != 0) return false;

        // �̿� ���� 1���� ū�� Ȯ��
        if (GetNeighbourCount(index) > 1) return false;

        // ���ݱ��� ������ ���� _maxRooms�� ������ ū�� Ȯ��
        if (_floorPlanCount > _maxRooms) return false;

        // ���������� �����Ͽ� ���� �ʹ� �������� �ʰ�, ������ ����(������)�� �ֱ� ���� ���� ��ġ
        // 50% Ȯ���� �� �� ����
        if (Random.value < 0.5f) return false;

        // ��� if ��� �� ������Ʈ �� �� �ִ� ��
        // Enqueue : ť�� �����͸� �ִ� ����
        _cellQueue.Enqueue(index);

        // �ش� ���� index 1�� �ٲٰ� ���� �� ���� + 1
        _floorPlan[index] = 1;
        _floorPlanCount++;

        // �� ����
        SpawnRoom(index);

        return true;
    }

    private void SpawnRoom(int index)
    {
        // index�� x, y ��ǥ�� ��ȯ
        int x = index % 10;
        int y = index / 10;

        // Unity�� y�� ������ ��� �Ʒ����� ����
        // ���� 2D map ���� �� 'ȭ�� �� ������ ���������� �����´�' ��� �����ϱ⿡ -y -> y = 0�� Unity ��ǥ�迡�� �� �� ��
        // �� �� ���� ��ŭ ����߸���
        Vector2 position = new Vector2(x * _cellSize, -y * _cellSize);

        // Instantiate(prefab, pos, rot)
        // �̸� ���� ���� ������Ʈ ����(Prefab), ������ ������Ʈ ��ǥ(pos), ������ ������Ʈ ȸ����(rot)
        Cell newCell = Instantiate(CellPrefab, position, Quaternion.identity);

        // �� Ÿ���� ��Ÿ���� �� EX) 1: �Ϲ� �� 2: ���� �� etc
        newCell.value = 1;

        // �� ���� � ��ġ�� �ش��ϴ���
        newCell.index = index;

        _spawnCells.Add(newCell);
    }
}
