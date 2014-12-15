﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Nodeplay.Interfaces;
using System.ComponentModel;
using UnityEngine.EventSystems;
using Nodeplay.UI;
using System.Collections;

class PortView : BaseView<PortModel>
{
	
    protected override void Start()
    {
        base.Start();
		this.gameObject.AddComponent<EventConsumer>();
        PositionNewPort(this.gameObject);

    }

    /// <summary>
    /// a tempconnector that is drawn while dragging.
    /// </summary>
    private GameObject tempconnector;

    //TODO this method should be on portview
    public GameObject PositionNewPort(GameObject port)
    {

        //bb of go

        //might need to be owner model - this might work for now as NodeView and Model should be the common base classes
        var view = this.Model.Owner.GetComponent<NodeView>();
        var boundingBox = view.UI.renderer.bounds;
        port.transform.parent = Model.Owner.transform;
        port.transform.position = Model.Owner.transform.position;
        // move the port from the center to back or front depending on port type

        float direction;

        if (port.GetComponent<PortModel>().PortType == PortModel.porttype.input)
        {
            direction = -1f;
        }
        else
        {
            direction = 1f;
        }

		if (port.GetComponent<ExecutionPortModel>() != null){
			direction = direction *1.5f;
		}

        port.transform.Translate(0, 0, boundingBox.size.z * direction);
        port.transform.localScale = new Vector3(.33f, .33f, .33f);

        // now we need to move the port in relation up or down to all other ports,
        // and possibly adjust other ports as well

        List<PortModel> ports;

        if (port.GetComponent<PortModel>().PortType == PortModel.porttype.input)
        {
            ports = Model.Owner.Inputs.Concat(Model.Owner.ExecutionInputs.Cast<PortModel>()).ToList();
        }
        else
        {
			ports = Model.Owner.Outputs.Concat(Model.Owner.ExecutionOutputs.Cast<PortModel>()).ToList();
        }


        foreach (var currentport in ports)
        {
            var index = ports.IndexOf(currentport);
            Debug.Log(boundingBox.size.y);
            Debug.Log(index);
            Debug.Log(ports.Count);
            currentport.gameObject.transform.localPosition = new Vector3(currentport.gameObject.transform.localPosition.x,
                ((boundingBox.size.y * 1.5f) * ((float)index / (float)ports.Count)) * 2 - boundingBox.size.y * 1.5f,
            currentport.gameObject.transform.localPosition.z);
        }
        return port;
    }
    
    public override void OnDrop(PointerEventData pointerdata)
    {
		if(pointerdata.button == PointerEventData.InputButton.Left){
		//if we mouseUp over a port we need to check if we were connecting/dragging,
		// and then we'll instantiate a new connectorModel, the model will create it's own view
		// and the view will listen to its ports for property changes
		
		Debug.Log("Mouse up event handler called");
		
		Debug.Log("I" + Model.NickName + " was just dropped on");
		
		var startport = pointerdata.pointerPress.GetComponent<PortModel>();
        if (startport != null)
        {
            if (startport.PortType == Model.PortType)
            {
                Debug.Log("breaking out, you cant attached two same direction connectors");
                return;
            }

            //TODO we must also look if we're about to create a cyclic dependencey, we should return a blank state

            // if port is already connected then disconnect old port before creating new connector
            if (Model.IsConnected)
            {	

            }
				Model.Owner.GraphOwner.AddConnection(pointerdata.pointerDrag.GetComponent<PortModel>(),this.Model);
        }
   	 }
	}
    public override void OnPointerUp(PointerEventData pointerdata)
    {


        //handle this here for now:
        //destruction of temp connector
        if (tempconnector != null)
        {
            DestroyImmediate(tempconnector);
            tempconnector = null;
        }

        

    }

    //handler for dragging node event//
    //TODO there is a bug when dragging from an input to an output which destroys the other connectors from that output
    //fixes might include restricting connecting creation only if the we drag on outputs, and mouseup on inputs
    // or investingating why this is not working further, connector model is being destroyed but still refereneced
    public override void OnDrag(PointerEventData pointerdata)
    {
		if (pointerdata.button == PointerEventData.InputButton.Left){
            Vector3 to_point = ProjectCurrentDrag(dist_to_camera);

            if (tempconnector != null)
            {
                DestroyImmediate(tempconnector);
                tempconnector = null;
            }
            // since this is a port, we need to instantiate a new 
            //ConnectorView ( this is a temporary connector that we drag around in the UI)

            tempconnector = new GameObject("TempConnectorView");
            tempconnector.AddComponent<TempConnectorView>();
            tempconnector.GetComponent<TempConnectorView>().init(this.gameObject.transform.position, to_point);

        }

	}

}

