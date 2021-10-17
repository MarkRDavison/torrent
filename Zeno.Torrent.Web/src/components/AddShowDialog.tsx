import React, { useState } from "react";
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, MenuItem, Select, TextField } from "@material-ui/core";
import axios from "axios";
import config from "../util/config";
import { DispatchType, RootState } from "../store/Store";
import { connect } from "react-redux";
import { setShowAdded, ShowsState } from "../store/ShowReducer";
import { Show } from "../models/Show";

interface OwnProps {
    open: boolean
    onClose: () => void
}

interface StateProps {
    showState: ShowsState
}

interface DispatchProps {
    addShow: (show: Show) => void
}

interface FormData {
    name: string
    quality: string
}

type Props = OwnProps & StateProps & DispatchProps;

const _AddShowDialog = (props: Props): JSX.Element => {
    const [formData, setFormData] = useState<FormData>({
        name: '',
        quality: '720p'
    });
    const handleChange = (event: React.ChangeEvent<{name?: string | undefined, value: unknown}>) => {
        event.preventDefault();
        setFormData({
            ...formData,
            [event.target.name as string]: event.target.value
        });
    };

    return (
        <Dialog
            open={props.open}
            onClose={props.onClose}>
            <DialogTitle>Add Show</DialogTitle>
            <DialogContent>
                <DialogContentText>
                    Add the name of the show
                </DialogContentText>
                <TextField 
                    autoFocus
                    margin='dense'
                    name='name'
                    label='Name'
                    type='text'
                    fullWidth
                    onChange={handleChange}
                    value={formData.name} />
                <Select
                    name='quality'
                    fullWidth
                    onChange={handleChange}
                    value={formData.quality}>
                    <MenuItem key='720p' value='720p'>720p</MenuItem>
                    <MenuItem key='1080p' value='1080p'>1080p</MenuItem>
                </Select>
            </DialogContent>
            <DialogActions>
                <Button color="secondary" onClick={() => props.onClose()}>
                Cancel
                </Button>
                <Button color="primary" onClick={async () => {
                    const response = await axios.post(`${config.bff_base_uri}/api/show`, {
                        name: formData.name,
                        quality: formData.quality
                    }, {
                        withCredentials: true
                    });
                    props.addShow(response.data);
                    setFormData({
                        name: '',
                        quality: '720p'
                    });
                    props.onClose();
                }}>
                Add
                </Button>
            </DialogActions>
        </Dialog>
    );
};

const mapStateToProps = (state: RootState): StateProps => {
    return {
        showState: state.showState
    };
};

const mapDispatchToProps = (dispatch: DispatchType): DispatchProps => {
    return {
        addShow: (show: Show) => {
            dispatch(setShowAdded(show));
        }
    };
};

const AddShowDialog = connect(
    mapStateToProps,
    mapDispatchToProps
)(_AddShowDialog);
export default AddShowDialog;