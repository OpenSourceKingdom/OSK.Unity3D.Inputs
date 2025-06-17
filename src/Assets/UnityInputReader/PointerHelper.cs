using System.Collections.Generic;
using System.Linq;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Options;
using OSK.Inputs.UnityInputReader.Assets.UnityInputReader.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace OSK.Inputs.UnityInputReader.Assets.UnityInputReader
{
    public static class PointerHelper
    {
        #region Select

        public static IEnumerable<GameObject> SelectGameObjects(Camera camera, Vector3 pointerStart, Vector3 pointerEnd, 
            GameSelectionOptions options)
        {
            var selectedObjects = camera.orthographic
                ? SelectObjectsOrthographic(camera, pointerStart, pointerEnd, options)
                : SelectObjectsPerspective(camera, pointerStart, pointerEnd, options);

            return selectedObjects.TakeWhile(o => !o.TryGetComponent<PointerSelectionBlocker>(out var pointerBlocker) && !pointerBlocker.BlockPointerCast);
        }

        private static IEnumerable<GameObject> SelectObjectsOrthographic(Camera camera, Vector3 pointerStart, Vector3 pointerEnd,
            GameSelectionOptions options)
        {
            if (pointerStart == pointerEnd)
            {
                // z position is distance from camera, but we want to start the cast at the camera.
                var pointerStartWorldPoint = camera.ScreenToWorldPoint(new Vector3(pointerStart.x, pointerStart.y, 0));

                return Physics.RaycastAll(pointerStartWorldPoint,
                                          camera.transform.forward,
                                          options.PointerCastDepth)
                               .Select(hit => hit.transform.gameObject);
            }

            // z position is distance from camera
            var selectionPointerCenter = new Vector3((pointerStart.x + pointerEnd.x) / 2,
                                              (pointerStart.y + pointerEnd.y) / 2,
                                              options.PointerCastDepth / 2);

            var selectionWorldCenter = camera.ScreenToWorldPoint(selectionPointerCenter);

            var pixelsToWorld = camera.orthographicSize / (Screen.height / 2f);

            var halfBoxSize = new Vector3(Mathf.Abs(selectionPointerCenter.x - pointerStart.x) * pixelsToWorld,
                                          Mathf.Abs(selectionPointerCenter.y - pointerStart.y) * pixelsToWorld,
                                          options.PointerCastDepth / 2);

            return Physics.OverlapBox(selectionWorldCenter,
                          halfBoxSize,
                          Quaternion.LookRotation(camera.transform.forward, camera.transform.up))
                          .Select(collider => collider.gameObject);
        }

        private static IEnumerable<GameObject> SelectObjectsPerspective(Camera camera, Vector3 pointerStart, Vector3 pointerEnd, 
            GameSelectionOptions options)
        {
            if (pointerStart == pointerEnd)
            {
                // z position is distance from camera, but we want to start the cast at the camera.
                var pointerStartWorldPoint = camera.ScreenToWorldPoint(new Vector3(pointerStart.x, pointerStart.y, 0));
                var direction = camera.ScreenToWorldPoint(new Vector3(pointerStart.x, pointerStart.y, options.PointerCastDepth)) - pointerStartWorldPoint;

                return Physics.RaycastAll(pointerStartWorldPoint,
                                          direction,
                                          options.PointerCastDepth)
                              .Select(hit => hit.transform.gameObject);
            }

            var halfDistance = options.PointerCastDepth / 2;

            // z position is distance from camera
            var startWorldPosition = camera.ScreenToWorldPoint(new Vector3(pointerStart.x, pointerStart.y, halfDistance));
            var endWorldPosition = camera.ScreenToWorldPoint(new Vector3(pointerEnd.x, pointerEnd.y, halfDistance));

            var centerWorldPosition = (startWorldPosition + endWorldPosition) / 2;

            var halfExtents = new Vector3(Mathf.Abs(startWorldPosition.x - endWorldPosition.x) / 2,
                                          Mathf.Abs(startWorldPosition.y - endWorldPosition.y) / 2,
                                          halfDistance);

            return Physics.OverlapBox(centerWorldPosition,
                                      halfExtents,
                                      Quaternion.LookRotation(camera.transform.forward, camera.transform.up))
                          .Select(collider => collider.gameObject);
        }

        #endregion

        #region UI

        public static bool IsPointerOverUIObject(Vector2 pointerLocation, UISelectionOptions options)
            => SelectUIObjects(pointerLocation, options).Any();

        public static IEnumerable<GameObject> SelectUIObjects(Vector2 pointerLocation, UISelectionOptions options)
        {
            var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = pointerLocation;
            var resultList = new List<RaycastResult>();

            var eventSystem = options.EventSystem ?? EventSystem.current;
            eventSystem.RaycastAll(eventDataCurrentPosition, resultList);

            var resultEnumerable = (IEnumerable<RaycastResult>)resultList;
            if (options.LayerFilter is not null && options.LayerFilter.Any())
            {
                var layerMaskLookup = options.LayerFilter.ToHashSet();
                resultEnumerable = resultList.Where(result => layerMaskLookup.Contains(result.gameObject.layer));
            }

            return resultEnumerable.Where(result => !result.gameObject.TryGetComponent<PointerSelectionBlocker>(out var bypassPointerUIBlock) || bypassPointerUIBlock.BlockPointerCast)
                .Select(result => result.gameObject);
        }

        #endregion
    }
}