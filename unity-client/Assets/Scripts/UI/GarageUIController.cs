using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NitroRush.UI
{
    public class CarDataComparer : IComparer<CarData>
    {
        public int Compare(CarData x, CarData y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            int rarityComparison = y.rarity.CompareTo(x.rarity);
            if (rarityComparison != 0) return rarityComparison;

            return x.price.CompareTo(y.price);
        }
    }

    /// <summary>
    /// Garage UI Controller managing vehicle browsing, sorting, stat visualization, and upgrades.
    /// </summary>
    public class GarageUIController : MonoBehaviour
    {
        [Header("Car Selection")]
        [SerializeField] private List<CarData> availableCars = new List<CarData>();
        [SerializeField] private Button purchaseButton;
        [SerializeField] private Text purchaseButtonText;

        [Header("Stat Bars")]
        [SerializeField] private Image speedBar;
        [SerializeField] private Image accelerationBar;
        [SerializeField] private Image handlingBar;
        [SerializeField] private Image nitroBar;

        [Header("Currencies")]
        [SerializeField] private Text creditsText;
        [SerializeField] private Text tokensText;

        private int _userCredits = 50000;
        private int _selectedCarIndex = 0;

        private void Start()
        {
            SortCars();
            UpdateCarUI();
        }

        /// <summary>
        /// Sorts available cars by rarity descending and price ascending.
        /// Sort Cost: List.Sort uses QuickSort T(N) = O(N log N) on average, O(N^2) worst case.
        /// </summary>
        public void SortCars()
        {
            availableCars.Sort(new CarDataComparer());
        }

        public void SelectCar(int index)
        {
            if (index >= 0 && index < availableCars.Count)
            {
                _selectedCarIndex = index;
                UpdateCarUI();
            }
        }

        private void UpdateCarUI()
        {
            if (availableCars.Count == 0) return;

            CarData car = availableCars[_selectedCarIndex];

            if (speedBar != null) speedBar.fillAmount = car.topSpeed / 350f;
            if (accelerationBar != null) accelerationBar.fillAmount = car.acceleration / 100f;
            if (handlingBar != null) handlingBar.fillAmount = car.handling / 100f;
            if (nitroBar != null) nitroBar.fillAmount = car.nitroCapacity / 100f;

            if (_userCredits < car.price)
            {
                if (purchaseButton != null) purchaseButton.interactable = false;
                if (purchaseButtonText != null) purchaseButtonText.text = "Insufficient Credits";
            }
            else
            {
                if (purchaseButton != null) purchaseButton.interactable = true;
                if (purchaseButtonText != null) purchaseButtonText.text = $"BUY (${car.price})";
            }

            if (creditsText != null) creditsText.text = $"Credits: {_userCredits}";
        }
    }
}
