﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DevExpress.Web.ASPxScheduler;
using DevExpress.XtraScheduler;

public partial class _Default : System.Web.UI.Page 
{
    ASPxSchedulerStorage Storage { get { return this.ASPxScheduler1.Storage; } }
    public static Random RandomInstance = new Random();

    protected void Page_Load(object sender, EventArgs e) {
        SetupMappings();
        ResourceFiller.FillResources(this.ASPxScheduler1.Storage, 3);

        ASPxScheduler1.AppointmentDataSource = appointmentDataSource;
        ASPxScheduler1.DataBind();
    }

#region #initclientappointment
    protected void ASPxScheduler1_InitClientAppointment(object sender, InitClientAppointmentEventArgs args) {
        args.Properties.Add(ClientSideAppointmentFieldNames.Description, args.Appointment.Description);
        args.Properties.Add(ClientSideAppointmentFieldNames.Subject, args.Appointment.Subject);
        args.Properties.Add("amount", args.Appointment.CustomFields["Field1"]);
    }
#endregion #initclientappointment

    #region Data Fill
    CustomEventList GetCustomEvents() {
        CustomEventList events = Session["ListBoundModeObjects"] as CustomEventList;
        if (events == null) {
            events = GenerateCustomEventList();
            Session["ListBoundModeObjects"] = events;
        }
        return events;
    }


    #region Random events generation
    CustomEventList GenerateCustomEventList() {
        CustomEventList eventList = new CustomEventList();
        int count = Storage.Resources.Count;
        for (int i = 0; i < count; i++) {
            Resource resource = Storage.Resources[i];
            string subjPrefix = resource.Caption + "'s ";

            eventList.Add(CreateEvent(resource.Id, subjPrefix + "meeting", 2, 5));
            eventList.Add(CreateEvent(resource.Id, subjPrefix + "travel", 3, 6));
            eventList.Add(CreateEvent(resource.Id, subjPrefix + "phone call", 0, 10));
        }
        return eventList;
    }
    CustomEvent CreateEvent(object resourceId, string subject, int status, int label) {
        CustomEvent customEvent = new CustomEvent();
        customEvent.Subject = subject;
        customEvent.Description = subject + ". " + "Some description.";
        customEvent.OwnerId = resourceId;
        Random rnd = RandomInstance;
        int rangeInHours = 48;
        customEvent.StartTime = DateTime.Today + TimeSpan.FromHours(rnd.Next(0, rangeInHours));
        customEvent.EndTime = customEvent.StartTime + TimeSpan.FromHours(rnd.Next(0, rangeInHours / 8));
        customEvent.Status = status;
        customEvent.Label = label;
        customEvent.Id = "ev" + customEvent.GetHashCode();
        customEvent.Amount = rnd.Next(1, 10);
        return customEvent;
    }
    #endregion

    void SetupMappings() {
        ASPxAppointmentMappingInfo mappings = Storage.Appointments.Mappings;
        Storage.BeginUpdate();
        try {
            mappings.AppointmentId = "Id";
            mappings.Start = "StartTime";
            mappings.End = "EndTime";
            mappings.Subject = "Subject";
            mappings.AllDay = "AllDay";
            mappings.Description = "Description";
            mappings.Label = "Label";
            mappings.Location = "Location";
            mappings.RecurrenceInfo = "RecurrenceInfo";
            mappings.ReminderInfo = "ReminderInfo";
            mappings.ResourceId = "OwnerId";
            mappings.Status = "Status";
            mappings.Type = "EventType";
            #region #custommappings
            Storage.Appointments.CustomFieldMappings.Add
                (new ASPxAppointmentCustomFieldMapping("Field1", "Amount"));
            Storage.Appointments.CustomFieldMappings.Add
                (new ASPxAppointmentCustomFieldMapping("Field2", "Memo"));
            #endregion #custommappings
        }
        finally {
            Storage.EndUpdate();
        }
    }
    protected void ASPxScheduler1_AppointmentInserting(object sender, PersistentObjectCancelEventArgs e) {
        ASPxSchedulerStorage storage = (ASPxSchedulerStorage)sender;
        Appointment apt = (Appointment)e.Object;
        storage.SetAppointmentId(apt, "a" + apt.GetHashCode());
    }
    protected void appointmentsDataSource_ObjectCreated(object sender, ObjectDataSourceEventArgs e) {
        e.ObjectInstance = new CustomEventDataSource(GetCustomEvents());
    }
    #endregion    
}
