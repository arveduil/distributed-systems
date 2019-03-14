package com.company;

public class Main {
    private static final int MULTICAST_PORT1 = 5007;
    private static final int MULTICAST_PORT2 = 5008;
    private static final String LOGGER_FILE_NAME_1 = "logger1.txt";
    private static final String LOGGER_FILE_NAME_2 = "logger2.txt";

    private static String MULTICAST_IP = "224.1.1.1";

    public static void main(String args[]) throws Exception
    {
        Runnable logger1 = new Logger(MULTICAST_PORT1,LOGGER_FILE_NAME_1);
        new Thread(logger1).start();

        Runnable logger2 = new Logger(MULTICAST_PORT2,LOGGER_FILE_NAME_2);
        new Thread(logger2).start();
    }


}
