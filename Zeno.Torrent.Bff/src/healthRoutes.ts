import axios from 'axios';
import express from 'express';

const createHealthRoutes = (): express.Router => {
    const router = express.Router();
    
    router.get('/up', async (req, res) => {
        res.send('OK');
    });

    router.get('/ready', async (req, res) => {
        res.send('OK');
    });

    return router;
};

export default createHealthRoutes;