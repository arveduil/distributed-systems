package com.company;

import java.io.BufferedWriter;
import java.io.FileWriter;
import java.io.IOException;
import java.net.DatagramPacket;
import java.net.InetAddress;
import java.net.MulticastSocket;
import java.nio.charset.StandardCharsets;

public class Logger implements Runnable {

    private  int port;
    private static String MULTICAST_IP = "224.1.1.1";
    private String fileName;


    public Logger(int port, String fileName) {
        this.port = port;
        this.fileName = fileName;
    }

    public void run() {
        try {
            createLoggger(port);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    private   void createLoggger(int port) throws IOException {
        byte[] b = new byte[1024];
        DatagramPacket dgram = new DatagramPacket(b, b.length);
        MulticastSocket socket = new MulticastSocket(port);
        socket.joinGroup(InetAddress.getByName(MULTICAST_IP));
        BufferedWriter writer = new BufferedWriter(new FileWriter(fileName));
        while(true) {
            socket.receive(dgram);
            String message = new String(dgram.getData(), StandardCharsets.UTF_8);
            writer.write(message + "\n");

            System.out.println("Received " + message);
            dgram.setLength(b.length);
        }
    }
}