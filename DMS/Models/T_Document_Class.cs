    using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace DMS.Models
{
    public class T_Document_Class {

        [Key]
        public int? SEQ { get; set; }
        public int? P_SEQ { get; set; }
        public string? NAME { get; set; }
        public int? LEVEL { get; set; }
        public int? ORDER { get; set; }

        [NotMapped]
        public List<T_Document_Class> Children { get; set; } = new List<T_Document_Class>();
        [NotMapped]
        public int? DOC_COUNT { get; set; }

        //// 트리 구조로 변환하는 메서드
        //public List<T_Document_Class> BuildTree(List<T_Document_Class> documentClasses)
        //{
        //    var menuItems = new List<T_Document_Class>();

        //    // 루트 항목들 (ParentId가 null인 항목)
        //    var rootItems = documentClasses.Where(x => x.P_SEQ == null).ToList();

        //    foreach (var rootItem in rootItems)
        //    {
        //        var childItems = documentClasses.Where(x => x.P_SEQ == rootItem.SEQ).ToList();
        //        rootItem.Children.AddRange(childItems);

        //        // 트리 구조로 추가
        //        menuItems.Add(rootItem);
        //    }

        //    return menuItems;
        //}

        public List<T_Document_Class> BuildTree(List<T_Document_Class> documentClasses)
        {
            var menuItems = new List<T_Document_Class>();

            // 루트 항목들 (부모가 없는 항목)
            var rootItems = documentClasses.Where(x => x.P_SEQ == null).ToList();

            foreach (var rootItem in rootItems)
            {
                // 자식 노드 재귀적으로 빌드
                AddChildren(rootItem, documentClasses);
                menuItems.Add(rootItem);
            }

            return menuItems;
        }

        // 재귀 메서드: 자식 노드를 찾아 트리 구조로 추가
        private void AddChildren(T_Document_Class parent, List<T_Document_Class> allItems)
        {
            var children = allItems.Where(x => x.P_SEQ == parent.SEQ).ToList();

            foreach (var child in children)
            {
                AddChildren(child, allItems); // 자식의 자식도 찾기
                parent.Children.Add(child);
            }
        }

    }



}

