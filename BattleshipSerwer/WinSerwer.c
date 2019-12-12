#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <winsock.h>


#define buffSize 10240
#define smallSize 8
struct Players
{
	int id;
	SOCKET playersocket;
	struct sockaddr_in endpoint;
	int playerType; //0 - player 1, 1 - player 2, 2 - guest
};
void clearBuf(char* b)
{
	int i;
	for (i = 0; i < buffSize; i++)
		b[i] = '\0';
}
void sendBig(struct Players player)
{
	char net_buf[buffSize];
	FILE* fp;
	int packet = 0;
	int size;
	int read;
	int sent;
	int sentfull;
	clearBuf(net_buf);
	if (player.playerType == 0)
		fp = fopen("niceimage.png", "rb");
	else
		fp = fopen("Explosion.jpg", "rb");
	if (fp == NULL)
		printf("\nFile open failed!\n");
	else
		printf("\nFile Successfully opened!\n");
	fseek(fp, 0, SEEK_END);
	size = ftell(fp);
	fseek(fp, 0, SEEK_SET);
	printf("Total Picture size: %i\n", size);


	printf("Sending Picture Size\n");
	sent = sendto(player.playersocket, (void *)&size, sizeof(int), 0, (struct sockaddr*) &player.endpoint, sizeof(struct sockaddr_in));
	printf("Packet Number: %i\n", packet);
	printf("Packet Size read: %i\n", sent);		
	printf(" \n");
	printf(" \n");

	if (sent < 0)
	{
		printf("Rozmiar nie wyslany");
	}
	while (!feof(fp)) 
	{
		read = fread(net_buf, 1, sizeof(net_buf), fp);
		if (ferror(fp) != 0)
			printf("Blad zapisu danych do pliku.\n");
		//Send data through our socket 
		sent = 0;
		sentfull = 0;
		do {
			sent = sendto(player.playersocket, net_buf, read,0, (struct sockaddr*) &player.endpoint, sizeof(struct sockaddr_in));
			sentfull += sent;
		} while (sent < 0);
		packet++;
		printf("Packet Number: %i\n", packet);
		printf("Packet Size read: %i\n", read);
		printf("Packet Size Sent: %i\n", sentfull);
		printf(" \n");
		printf(" \n");
		//Zero out our send buffer
		memset(net_buf, 0, buffSize);
	}
	return;
}
void message(char* tab)
{
	for (int i = 0; i < 7; i++)
		tab[i] = '0';
}


int main(int argc, char **argv)
{
	WSADATA wsaData; // je´sli to nie zadzia³a
	//WSAData wsaData; // uÿzyj tego
	if (WSAStartup(MAKEWORD(1, 1), &wsaData) != 0) {
		fprintf(stderr, "WSAStartup failed.\n");
		exit(1);
	}
	char recbuf[8];
	char sendbuf[8];
	struct Players players[10];
	int plays = 0;
	int watches = 2;
	int owner = 0, recipient = 0;
	int ready = 0;
	struct sockaddr_in myaddr, remoteaddr;
	int socketAmount;
	SOCKET sdsocket;
	int addrlen;
	int r, sent;
	unsigned int port = 8888;
	fd_set desset;
	if ((sdsocket = socket(AF_INET, SOCK_DGRAM, 0)) < 0)
	{
		printf("socket() nie powiodl sie\n");
		return 1;
	}

	myaddr.sin_family = AF_INET;
	myaddr.sin_port = htons(port);
	myaddr.sin_addr.s_addr = INADDR_ANY;

	if (bind(sdsocket, (struct sockaddr*) &myaddr, sizeof(struct sockaddr_in)) < 0)
	{
		printf("bind() nie powiodl sie\n");
		perror(WSAGetLastError);
		return 1;
	}
	addrlen = sizeof(struct sockaddr_in);


	// turururu
	socketAmount = sdsocket + 1;
	FD_ZERO(&desset);
	FD_SET(sdsocket, &desset);

	printf("Serwer wystartowal\n");
	while (1)
	{
		memset(recbuf, 0, 8);
		r = 0;
		printf("\n\nSerwer oczekuje\n");
		while (r < 7)
			r += recvfrom(sdsocket, recbuf + r, 8 - r, 0, (struct sockaddr*) &remoteaddr, &addrlen);
		recbuf[7] = '\0';
		printf("\n\nBitow odebranych %d. Wiadomosc od %s: %s\n",
			r,
			inet_ntoa(remoteaddr.sin_addr),
			recbuf);
		if (strcmp(recbuf, "0000000") == 0)
		{
			printf("Prosba o id\n");
			if (plays < 2)
			{
				players[plays].id = plays;
				players[plays].playersocket = sdsocket;
				players[plays].endpoint = remoteaddr;
				players[plays].playerType = plays;
				memset(sendbuf, 0, 8);
				message(sendbuf);
				sendbuf[2] = plays + '0';
				sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &players[plays].endpoint, addrlen);
				printf("Id wyslane %s\n", sendbuf);
				plays++;
			}
			else
			{
				memset(sendbuf, 0, 8);
				message(sendbuf);
				sendbuf[0] = '1';
				sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &remoteaddr, addrlen);
				printf("Limit graczy osiagniety\n");
			}
		}
		else if (strcmp(recbuf, "0010000") == 0)
		{
			if (watches < 10)
			{
				players[watches].id = watches;
				players[watches].playersocket = sdsocket;
				players[watches].endpoint = remoteaddr;
				players[watches].playerType = 2;
				memset(sendbuf, 0, 8);
				message(sendbuf);
				sendbuf[2] = watches + '0';
				sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &players[watches].endpoint, addrlen);
				printf("Id wyslane %s\n", sendbuf);
				watches++;
			}
			else
			{
				memset(sendbuf, 0, 8);
				message(sendbuf);
				sendbuf[0] = '1';
				sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &remoteaddr, addrlen);
				printf("Limit ogladajacych osiagniety\n");
			}
		}
		else if (strcmp(recbuf, "0099900") == 0)
		{
			printf("Wiadomosc Koniec utworzona %s\n", recbuf);
			for (int i = 0; i < watches; i++)
			{
				if (plays == 1 && i == 1) continue;
				r = 0;

				while (r < 7)
				{
					sent = sendto(players[i].playersocket, recbuf + r, 8 - r, 0,
						(struct sockaddr*) &players[i].endpoint, addrlen);
					if (sent == 0 || sent == -1)
					{
						printf("Wiadomosc napotkala problem %d\n", r);
						if (sent == -1)
							perror(send);
						break;
					}
					r += sent;
				}

				printf("Wiadomosc koniec wyslany %d\n", r);
			}
			for (int i = 0; i < watches; i++)
			{
				if (plays == 1 && i == 1) continue;
				sendBig(players[i]);
			}
			return;
		}
		else if (strcmp(recbuf, "0022200") == 0)
		{
			++ready;
			if (ready >= 2)
			{
				for (int i = 0; i < plays; i++)
				{

					r = 0;
					sendbuf[1] = i + '0';
					sendbuf[2] = '2';
					sendbuf[3] = '2';
					sendbuf[4] = '2';
					while (r < 8)
					{
						sent = sendto(players[i].playersocket, sendbuf + r, 8 - r, 0,
							(struct sockaddr*) &players[i].endpoint, addrlen);
						if (sent == 0 || sent == -1)
						{
							printf("Wiadomosc napotkala problem %d\n", r);
							if (sent == -1)
								perror(send);
							break;
						}
						r += sent;
					}

					printf("Wiadomosc poczatek wyslany %d\n", r);
				}
			}
		}
		else
		{
			if (plays != 2)
			{
				printf("Oczekujemy nastepnego gracza / bledna wiadomosc \n");
				memset(sendbuf, 0, 8);
				message(sendbuf);
				sendbuf[0] = '1';
				sent = sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &remoteaddr, addrlen);
				if (sent == 0 || sent == -1)
				{
					printf("Error napotkal problem %d\n", r);
					if (sent == -1)
						perror(send);
				}
				continue;
			}
			if (recbuf[0] == '0')
			{
				owner = recbuf[1] - '0';
				recipient = (owner + 1) % 2;
				printf("Poprawna wiadomosc dostana %s, %d, %d\n", recbuf, owner, recipient);
				if (recbuf[3] == '1')
				{
					printf("Strzal otrzymany\n");
					memset(sendbuf, 0, 8);
					message(sendbuf);
					sendbuf[1] = recipient + '0';
					sendbuf[2] = owner + '0';
					sendbuf[3] = '2';
					sendbuf[5] = recbuf[5];
					sendbuf[6] = recbuf[6];
					printf("Wiadomosc strzal utworzona %s\n", sendbuf);
					r = 0;
					while (r < 7)
					{

						sent = sendto(players[recipient].playersocket, sendbuf + r, 8 - r, 0,
							(struct sockaddr*) &players[recipient].endpoint, addrlen);
						if (sent == 0 || sent == -1)
						{
							printf("Strzal napotkal problem %d\n", r);
							if (sent == -1)
								perror(send);
							break;
						}
						r += sent;
					}
					printf("Strzal wyslany %d\n", r);
				}
				else if (recbuf[3] == '2')
				{
					printf("Wynik otrzymany\n");
					for (int i = 0; i < watches; i++)
					{
						if (i == owner) continue;
						memset(sendbuf, 0, 8);
						message(sendbuf);
						sendbuf[1] = i + '0';
						sendbuf[2] = owner + '0';
						sendbuf[3] = '1';
						sendbuf[4] = recbuf[4];
						sendbuf[5] = recbuf[5];
						sendbuf[6] = recbuf[6];
						printf("Wiadomosc wynik utworzona %s\n", sendbuf);
						r = 0;
						while (r < 8)
						{
							sent= sendto(players[i].playersocket, sendbuf + r, 8 - r, 0,
							(struct sockaddr*) &players[i].endpoint, addrlen);
							if (sent == 0 || sent == -1)
							{
								printf("Wynik napotkal problem %d\n", r);
								if (sent == -1)
									perror(send);
								break;
							}
							r += sent;
						}
						printf("Wynik wyslany %d\n", r);
					}
				}
				else
				{
					memset(sendbuf, 0, 8);
					message(sendbuf);
					sendbuf[0] = '1';
					sent = sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &remoteaddr, addrlen);
					if (sent == 0 || sent == -1)
					{
						printf("Error napotkal problem %d\n", r);
						if (sent == -1)
							perror(send);
					}
					printf("Zly format \n");
				}

			}

			else
			{
				memset(sendbuf, 0, 8);
				message(sendbuf);
				sendbuf[0] = '1';
				sent = sendto(sdsocket, sendbuf, 8, 0, (struct sockaddr*) &remoteaddr, addrlen);
				if (sent == 0 || sent == -1)
				{
					printf("Error napotkal problem %d\n", r);
					if (sent == -1)
						perror(send);
				}
				printf("Zly format \n");
			}
		}
	}
	return 0;

}