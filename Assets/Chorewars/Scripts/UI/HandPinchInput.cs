using System;
using System.Reflection;
using UnityEngine;

namespace Chorewars.UI
{
    internal static class HandPinchInput
    {
        private static float _nextRescanAt;
        private static object _leftHand;
        private static object _rightHand;
        private static bool _leftWasPinching;
        private static bool _rightWasPinching;

        private static Type _ovrHandType;
        private static MethodInfo _getFingerIsPinching;
        private static PropertyInfo _handTypeProp;
        private static Type _handFingerEnum;
        private static object _indexFingerEnumValue;

        public static bool TryGetPinchDown(out bool leftPinchDown, out bool rightPinchDown)
        {
            leftPinchDown = false;
            rightPinchDown = false;

            if (!EnsureBound()) return false;

            bool leftIsPinching = _leftHand != null && GetIndexPinching(_leftHand);
            bool rightIsPinching = _rightHand != null && GetIndexPinching(_rightHand);

            leftPinchDown = leftIsPinching && !_leftWasPinching;
            rightPinchDown = rightIsPinching && !_rightWasPinching;

            _leftWasPinching = leftIsPinching;
            _rightWasPinching = rightIsPinching;

            return true;
        }

        private static bool EnsureBound()
        {
            float now = Time.unscaledTime;
            if (_ovrHandType == null)
            {
                _ovrHandType = Type.GetType("OVRHand, Assembly-CSharp", throwOnError: false)
                             ?? Type.GetType("OVRHand, Oculus.VR", throwOnError: false);

                if (_ovrHandType == null) return false;

                _getFingerIsPinching = _ovrHandType.GetMethod("GetFingerIsPinching", BindingFlags.Instance | BindingFlags.Public);
                _handTypeProp = _ovrHandType.GetProperty("HandType", BindingFlags.Instance | BindingFlags.Public);
                _handFingerEnum = _ovrHandType.GetNestedType("HandFinger", BindingFlags.Public);

                if (_getFingerIsPinching == null || _handFingerEnum == null) return false;
                _indexFingerEnumValue = Enum.Parse(_handFingerEnum, "Index");
            }

            if (now < _nextRescanAt && (_leftHand != null || _rightHand != null))
                return true;

            _nextRescanAt = now + 1.0f;
            BindHandsFromScene();
            return _leftHand != null || _rightHand != null;
        }

        private static void BindHandsFromScene()
        {
            _leftHand = null;
            _rightHand = null;

            // FindAnyObjectByType can't target a runtime Type, so use FindObjectsOfTypeAll + filter by name.
            var behaviours = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                var mb = behaviours[i];
                if (mb == null) continue;
                var t = mb.GetType();
                if (!ReferenceEquals(t, _ovrHandType)) continue;

                if (TryClassifyHand(mb, out bool isLeft, out bool isRight))
                {
                    if (isLeft && _leftHand == null) _leftHand = mb;
                    if (isRight && _rightHand == null) _rightHand = mb;
                }
                else
                {
                    // Fallback assignment if HandType isn't available.
                    if (_leftHand == null) _leftHand = mb;
                    else if (_rightHand == null) _rightHand = mb;
                }

                if (_leftHand != null && _rightHand != null) break;
            }
        }

        private static bool TryClassifyHand(object ovrHand, out bool isLeft, out bool isRight)
        {
            isLeft = false;
            isRight = false;
            if (_handTypeProp == null) return false;

            try
            {
                var v = _handTypeProp.GetValue(ovrHand);
                if (v == null) return false;
                var s = v.ToString();
                if (string.IsNullOrWhiteSpace(s)) return false;

                isLeft = s.IndexOf("Left", StringComparison.OrdinalIgnoreCase) >= 0;
                isRight = s.IndexOf("Right", StringComparison.OrdinalIgnoreCase) >= 0;
                return isLeft || isRight;
            }
            catch
            {
                return false;
            }
        }

        private static bool GetIndexPinching(object ovrHand)
        {
            try
            {
                object[] args = { _indexFingerEnumValue };
                var result = _getFingerIsPinching.Invoke(ovrHand, args);
                return result is bool b && b;
            }
            catch
            {
                return false;
            }
        }
    }
}

