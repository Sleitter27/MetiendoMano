﻿using SGC_MVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using SGC_MVC.Models.ViewModels;
using SGC_MVC.CustomCode;
using System.Data;
using WebMatrix.WebData;

namespace SGC_MVC.Controllers
{

    [IsMenu]
    [CustomAuthorize]
    public class PoliciesController : System.Web.Mvc.Controller
    {
        SASContext db = new SASContext();
        //
        // GET: /Policies/
        [IsView]
        public ActionResult Index()
        {
            int? companyID = db.Users.Find(WebSecurity.CurrentUserId).companyID;
            var policies = db.Documents.Where(dep => dep.companyID == companyID).Include(d => d.Rules).Include(d => d.Status)
                .Where(d => d.DocumentType.name == "Politica" && d.documentParentID == null);

            return View(policies.ToList());
        }

        [IsView]
        public ActionResult Create()
        {
            DocumentEditViewModel model = new DocumentEditViewModel();
            model.document.documentTypeID = (
                        from d in db.DocumentTypes
                        where d.name == "Politica"
                        select d.ID
                    ).FirstOrDefault();
            ViewBag.allRules = Helper.GetRulesSelect(db);

            return View(model);
        }

        [HttpPost]
        public ActionResult Create(DocumentEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.document.Rules.Clear();

                model.document.version = 1;
                model.document.createUser = WebSecurity.CurrentUserId;
                model.document.companyID = (int)db.Users.Find(WebSecurity.CurrentUserId).companyID;
                model.document.statusID = 1;
                Helper.updateObjectFields(model.document);

                if (model.selectedRules != null)
                    foreach (int ruleId in model.selectedRules)
                    {
                        model.document.Rules.Add(db.Rules.Find(ruleId));
                    }

                db.Documents.Add(model.document);
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = model.document.ID });
            }

            ViewBag.allRules = Helper.GetRulesSelect(db);
            return View(model);
        }

        [IsView]
        public ActionResult Edit(int id = 0)
        {
            DocumentEditViewModel model = new DocumentEditViewModel();
            int? companyID = db.Users.Find(WebSecurity.CurrentUserId).companyID;
            model.document = db.Documents.Where(dep => dep.companyID == companyID).Include(d => d.Rules)
                .Include(d => d.Status).Include(d => d.Document1)
                .Include(d => d.Document2).FirstOrDefault(d => d.ID == id);

            ViewBag.allRules = Helper.GetRulesSelect(db);
            model.selectedRules = (from r in model.document.Rules
                                   select r.ID).ToArray();

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(DocumentEditViewModel model, string submitVal)
        {
            if (ModelState.IsValid)
            {
                Helper.updateObjectFields(model.document);
                model.document.Rules.Clear();
                model.document.updateDate = DateTime.Now;

                if (submitVal == "nueva version")
                {
                    HistDocument d = new HistDocument(model.document);
                    d.changeReason = model.changeReason;
                    db.HistDocuments.Add(d);
                    db.SaveChanges();
                    model.document.version++;
                }

                db.Entry(model.document).State = EntityState.Modified;
                db.SaveChanges();

                Document document = db.Documents.Include(d => d.Rules)
                   .FirstOrDefault(d => d.ID == model.document.ID);
                document.Rules.Clear();
                db.Entry(document).State = EntityState.Modified;
                db.SaveChanges();

                if (model.selectedRules != null)
                    foreach (int ruleId in model.selectedRules)
                    {
                        document.Rules.Add(db.Rules.Find(ruleId));
                    }

                db.Entry(document).State = EntityState.Modified;
                db.SaveChanges();

                if (submitVal == "Capitulo")
                {
                    return RedirectToAction("CreateChild", new { id = model.document.ID });
                }
                else
                    return RedirectToAction("Index");
            }
            ViewBag.allRules = Helper.GetRulesSelect(db);

            return View(model);
        }

        [IsView]
        public ActionResult CreateChild(int id = 0)
        {
            Document parent = db.Documents.Find(id);
            if (parent == null)
            {
                return HttpNotFound();
            }
            SubDocumentEditViewModel model = new SubDocumentEditViewModel();
            model.document.Document2 = parent;
            model.document.companyID = parent.companyID;
            model.document.documentTypeID = parent.documentTypeID;
            model.document.documentParentID = id;

            return View(model);
        }

        [HttpPost]
        public ActionResult CreateChild(SubDocumentEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                Document parent = db.Documents.Include(d => d.Rules)
                .Include(d => d.Document1).Include(d => d.Document2)
                .FirstOrDefault(d => d.ID == model.document.documentParentID);

                model.document.documentTypeID = parent.documentTypeID;
                Helper.updateObjectFields(model.document);
                double edt = 0;
                if (parent.Document1.Count() > 0)
                {
                    edt = (from d in parent.Document1
                           select d.EDT).Max();
                }
                model.document.EDT = edt + 1;
                model.document.createUser = WebSecurity.CurrentUserId;
                model.document.companyID = parent.companyID;
                db.Documents.Add(model.document);
                db.SaveChanges();

                return RedirectToAction("EditChild", new { id = model.document.ID });
            }

            return View(model);
        }

        [IsView]
        public ActionResult EditChild(int id = 0)
        {
            SubDocumentEditViewModel model = new SubDocumentEditViewModel();
            model.document = db.Documents.Include(d => d.Rules).FirstOrDefault(d => d.ID == id);
            return View(model);
        }

        [HttpPost]
        public ActionResult EditChild(SubDocumentEditViewModel model, string submitVal)
        {
            if (ModelState.IsValid)
            {
                model.document.updateDate = DateTime.Now;
                Helper.updateObjectFields(model.document);
                model.document.Rules.Clear();
                db.Entry(model.document).State = EntityState.Modified;
                db.SaveChanges();

                Document document = db.Documents.Include(d => d.Rules)
                    .FirstOrDefault(d => d.ID == model.document.ID);
                document.Rules.Clear();
                db.SaveChanges();

                //if (model.selectedRules != null)
                //    foreach (int ruleId in model.selectedRules)
                //    {
                //        document.Rules.Add(db.Rules.Find(ruleId));
                //    }

                db.Entry(document).State = EntityState.Modified;
                db.SaveChanges();
                if (submitVal == "Capitulo")
                {
                    return RedirectToAction("CreateChild", new { id = model.document.ID });
                }
                else
                {
                    if (db.Documents.Include(d => d.Document2)
                        .FirstOrDefault(d => d.ID == model.document.documentParentID)
                        .Document2 == null)
                    {
                        return RedirectToAction("Edit", new { id = model.document.documentParentID });
                    }
                    return RedirectToAction("EditChild", new { id = model.document.documentParentID });
                }

            }

            model.document.documentTypeID = (from d in db.DocumentTypes
                                             where d.name == "Politica"
                                             select d.ID).FirstOrDefault();

            ViewBag.allRules = Helper.GetRulesSelect(db);

            return View(model);
        }

        [IsView]
        public ActionResult DeleteChild(int id = 0)
        {
            Document doc = db.Documents.Include(d => d.Document2)
                .FirstOrDefault(d => d.ID == id);

            if (doc == null)
            {
                return HttpNotFound();
            }

            return View(doc);
        }

        [HttpPost, ActionName("DeleteChild")]
        public ActionResult DeleteChildConfirmed(int id)
        {
            Document doc = db.Documents.Include(d => d.Document2)
                .FirstOrDefault(d => d.ID == id);
            int parent_id = doc.documentParentID.Value;

            string action = (doc.Document2.Document2 == null)
                ? "Edit" : "EditChild";
            string query = "update document set EDT = EDT - 1"
                            + "where documentParentID = {0}"
                            + " and EDT > {1};";

            doc.Rules.Clear();
            if (doc.Document1 != null)
                foreach (Document docy in doc.Document1.ToList())
                {
                    db.Documents.Remove(docy);
                }

            db.Database.ExecuteSqlCommand(query, doc.documentParentID, doc.EDT);
            db.Documents.Remove(doc);
            db.SaveChanges();

            return RedirectToAction(action, new { id = parent_id });
        }

        [IsView]
        public ActionResult Delete(int id = 0)
        {
            var policy = db.Documents.Find(id);
            if (policy == null)
            {
                return HttpNotFound();
            }

            return View(policy);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var policy = db.Documents.Include(d => d.Document1).Include(d => d.Rules)
                .FirstOrDefault(d => d.ID == id);
            policy.Rules.Clear();

            if (policy.Document1 != null)
                foreach (Document d in policy.Document1.ToList())
                {
                    db.Documents.Remove(d);
                }
            policy.Document1.Clear();
            db.SaveChanges();
            db.Documents.Remove(policy);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Actualiza el orden de los capitulos usando el EDT
        /// </summary>
        /// <param name="parentID"></param>
        /// <param name="id"></param>
        /// <param name="fromPosition"></param>
        /// <param name="toPosition"></param>
        /// <param name="direction"></param>
        public void UpdateOrder(int parentID, int id, int fromPosition, int toPosition, string direction)
        {

            int? companyID = db.Users.Find(WebSecurity.CurrentUserId).companyID;
            if (direction == "back")
            {
                var movedDocuments = db.Documents.Where(dep => dep.companyID == companyID)
                            .Where(d => (toPosition <= d.EDT && d.EDT <= fromPosition)
                                   && d.documentParentID == parentID)
                            .ToList();

                foreach (var document in movedDocuments)
                {
                    document.EDT++;
                    db.Entry(document).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            else
            {
                var movedCompanies = db.Documents.Where(dep => dep.companyID == companyID)
                            .Where(d => (fromPosition <= d.EDT && d.EDT <= toPosition))
                            .ToList();
                foreach (var company in movedCompanies)
                {
                    company.EDT--;
                }
            }

            var doc = db.Documents.FirstOrDefault(d => d.ID == id);
            doc.EDT = toPosition;
            db.Entry(doc).State = EntityState.Modified;
            db.SaveChanges();

        }
    }
}
